using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Tweakbox.Components.Netplay.Helpers;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Constants = Riders.Netplay.Messages.Misc.Constants;
using Extensions = Riders.Tweakbox.Components.Netplay.Helpers.Extensions;

namespace Riders.Tweakbox.Components.Netplay.Components.Menu
{
    public unsafe class CharacterSelect : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }

        public CommonState State { get; private set; }
        public EventController Event { get; set; }
        public CharaSelectSync LastSync { get; private set; }

        /// <summary> Sync data for character select. </summary>
        private Timestamped<CharaSelectSync> _sync = new CharaSelectSync().CreatePooled(Constants.MaxNumberOfPlayers);
        private TimeStamp[] _stamps = new TimeStamp[Constants.MaxNumberOfPlayers];
        private ExitKind _exit = ExitKind.Null;
        private readonly byte _sequencedChannel;

        public CharacterSelect(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;
            State  = Socket.State;

            _sequencedChannel = (byte) Socket.ChannelAllocator.GetChannel(DeliveryMethod.ReliableSequenced);
            Event.OnCharacterSelect         += OnCharaSelect;
            Event.OnCheckIfExitCharaSelect  += MenuCheckIfExitCharaSelect;
            Event.OnExitCharaSelect         += MenuOnExitCharaSelect;
            Event.OnCheckIfStartRace        += MenuCheckIfStartRace;
            Event.OnStartRace               += MenuOnMenuStartRace;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Socket.ChannelAllocator.ReleaseChannel(DeliveryMethod.ReliableSequenced, _sequencedChannel);
            Event.OnCharacterSelect         -= OnCharaSelect;
            Event.OnCheckIfExitCharaSelect  -= MenuCheckIfExitCharaSelect;
            Event.OnExitCharaSelect         -= MenuOnExitCharaSelect;
            Event.OnCheckIfStartRace        -= MenuCheckIfStartRace;
            Event.OnStartRace               -= MenuOnMenuStartRace;
        }

        private void MenuOnMenuStartRace() => DoExitCharaSelect(ExitKind.Start);
        private void MenuOnExitCharaSelect() => DoExitCharaSelect(ExitKind.Exit);
        private Enum<AsmFunctionResult> MenuCheckIfStartRace() => _exit == ExitKind.Start;
        private Enum<AsmFunctionResult> MenuCheckIfExitCharaSelect() => _exit == ExitKind.Exit;

        private void DoExitCharaSelect(ExitKind kind)
        {
            if (_exit != kind)
            {
                if (Socket.GetSocketType() == SocketType.Host)
                    Socket.SendToAllAndFlush(ReliablePacket.Create(new CharaSelectExit(kind)), DeliveryMethod.ReliableOrdered, $"[{nameof(CharacterSelect)} / Host] Sending Start/Exit Flag to Clients", LogCategory.Menu);
                else
                    Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(new CharaSelectExit(kind)), DeliveryMethod.ReliableOrdered, $"[{nameof(CharacterSelect)} / Client] Sending Start/Exit flag to Host", LogCategory.Menu);
            }

            _exit = ExitKind.Null;
        }

        private unsafe void OnCharaSelect(Task<Sewer56.SonicRiders.Structures.Tasks.CharacterSelect, CharacterSelectTaskState>* task)
        {
            // Update local player info.
            for (int x = 0; x < State.NumLocalPlayers; x++)
                _sync.Value.Elements[x] = CharaSelectLoop.FromGame(task, x);

            // Discard outdated Character Selects
            for (int x = State.NumLocalPlayers; x < _stamps.Length; x++)
            {
                if (_stamps[x].IsDiscard(State.MaxLatency))
                    _sync.Value.Elements[x] = default;
            }

            // Synchronize with Host
            Synchronize(task);
            if (_exit != ExitKind.Null)
                return;

            switch (Socket.GetSocketType())
            {
                case SocketType.Host:
                {
                    var hostState = (HostState)Socket.State;
                    Span<byte> excludeIndexBuffer = stackalloc byte[Constants.MaxNumberOfLocalPlayers];

                    // Note: Do not use SendAndFlush here as not only is it inefficient, you risk
                    //       accessing ConnectedPeerList inside the message handler(s); which will break foreach.
                    
                    for (var x = 0; x < Socket.Manager.ConnectedPeerList.Count; x++)
                    {
                        var peer           = Socket.Manager.ConnectedPeerList[x]; 
                        var playerData     = hostState.ClientMap.GetPlayerData(peer);
                        var excludeIndices = playerData.GetExcludeIndices(excludeIndexBuffer);

                        using var rental   = Extensions.GetItemsWithoutIndices(_sync.Value.Elements.AsSpan(0, State.GetPlayerCount()), excludeIndices);
                        using var message  = new CharaSelectSync();
                        message.Set(rental.Segment.Array, rental.Length);

                        Socket.Send(peer, ReliablePacket.Create(message), DeliveryMethod.ReliableSequenced, _sequencedChannel);
                    }

                    break;
                }
                
                case SocketType.Client when State.NumLocalPlayers > 0:
                {
                    var loops         = _sync.Value.Elements.AsSpan(0, State.NumLocalPlayers);
                    using var message = new CharaSelectSync().CreatePooled(loops.Length);
                    loops.CopyTo(message.Elements);

                    Socket.Send(Socket.Manager.FirstPeer, ReliablePacket.Create(message), DeliveryMethod.ReliableSequenced, _sequencedChannel);
                    break;
                }
            }

            Socket.Update();
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
        {
            // Check message type.
            var messageType = packet.MessageType;
            switch (messageType)
            {
                case MessageType.CharaSelectExit:
                    HandleReliablePacketExit(ref packet, source);
                    break;
                case MessageType.CharaSelectSync:
                    HandleReliablePacketSync(ref packet, source);
                    break;
            }
        }

        private void HandleReliablePacketSync(ref ReliablePacket packet, NetPeer source)
        {
            // Get message.
            var sync = packet.GetMessage<CharaSelectSync>();

            // Index of first player to fill.
            int playerIndex = Socket.GetSocketType() switch
            {
                SocketType.Host   => ((HostState)State).ClientMap.GetPlayerData(source).PlayerIndex,
                SocketType.Client => State.NumLocalPlayers,
                _ => throw new ArgumentOutOfRangeException()
            };

            var loops    = sync.Elements;
            var numLoops = sync.NumElements;
            _sync.Refresh();

            for (int x = 0; x < numLoops; x++)
            {
                _sync.Value.Elements[x + playerIndex] = loops[x];
                _stamps[x + playerIndex].Refresh();
            }
        }

        private void HandleReliablePacketExit(ref ReliablePacket packet, NetPeer source)
        {
            var charaSelectExit = packet.GetMessage<CharaSelectExit>();
            
            Log.WriteLine($"[{nameof(CharacterSelect)}] Got Start/Exit Request Flag", LogCategory.Menu);
            if (Socket.GetSocketType() == SocketType.Host)
                Socket.SendToAllExceptAndFlush(source, ReliablePacket.Create(new CharaSelectExit(charaSelectExit.Type)), DeliveryMethod.ReliableOrdered);

            _exit = charaSelectExit.Type;
        }

        /// <summary>
        /// Common implementation for syncing character select events.
        /// </summary>
        /// <param name="task">Current character select task.</param>
        private unsafe void Synchronize(Task<Sewer56.SonicRiders.Structures.Tasks.CharacterSelect, CharacterSelectTaskState>* task)
        {
            if (_sync.IsDiscard(State.MaxLatency))
                return;

            LastSync = _sync.Value;
            _sync.Value.ToGame(task, State.NumLocalPlayers);
        }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
    }
}
