using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Constants = Riders.Netplay.Messages.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay.Components.Menu
{
    public unsafe class CharacterSelect : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        public CharaSelectSync LastSync { get; private set; }

        /// <summary> Sync data for character select. </summary>
        private Volatile<Timestamped<CharaSelectSync>> _sync = new Volatile<Timestamped<CharaSelectSync>>(new Timestamped<CharaSelectSync>());
        private Timestamped<CharaSelectLoop>[] _loops = new Timestamped<CharaSelectLoop>[Constants.MaxNumberOfPlayers];
        private ExitKind _exit = ExitKind.Null;
        private readonly byte _sequencedChannel;

        public CharacterSelect(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;

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

        private unsafe void OnCharaSelect(Task<Sewer56.SonicRiders.Structures.Tasks.CharacterSelect, CharacterSelectTaskState>* task)
        {
            CharaSelectLoop[] sync = new CharaSelectLoop[0];
            if (Socket.GetSocketType() == SocketType.Host)
            {
                sync = GetCharacterSelect();
                sync[0] = CharaSelectLoop.FromGame(task);
                _sync = new Volatile<Timestamped<CharaSelectSync>>(new CharaSelectSync(sync.Where((loop, x) => x != 0).ToArray()));
            }

            Synchronize(task);
            if (_exit != ExitKind.Null)
                return;

            switch (Socket.GetSocketType())
            {
                case SocketType.Host:
                    var state = (HostState)Socket.State;
                    // Note: Do not use SendAndFlush here as not only is it inefficient, you risk
                    //       accessing ConnectedPeerList inside the message handler(s); which will break foreach.
                    for (var x = 0; x < Socket.Manager.ConnectedPeerList.Count; x++)
                    {
                        var peer = Socket.Manager.ConnectedPeerList[x];
                        var excludeIndex = state.ClientMap.GetPlayerData(peer).PlayerIndex;
                        var selectSync = new CharaSelectSync(sync.Where((loop, x) => x != excludeIndex).ToArray());
                        Socket.Send(peer, new ReliablePacket(selectSync), DeliveryMethod.ReliableSequenced, _sequencedChannel);
                    }

                    Socket.Update();
                    break;

                case SocketType.Client:
                    Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket(CharaSelectLoop.FromGame(task)), DeliveryMethod.ReliableSequenced, _sequencedChannel);
                    break;

                case SocketType.Spectator: break;
            }
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
                    Socket.SendToAllAndFlush(new ReliablePacket(new CharaSelectExit(kind)), DeliveryMethod.ReliableOrdered, $"[{nameof(CharacterSelect)} / Host] Sending Start/Exit Flag to Clients", LogCategory.Menu);
                else
                    Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket(new CharaSelectExit(kind)), DeliveryMethod.ReliableOrdered, $"[{nameof(CharacterSelect)} / Client] Sending Start/Exit flag to Host", LogCategory.Menu);
            }

            _exit = ExitKind.Null;
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet)
        {
            if (!packet.TryGetPacket<ReliablePacket>(Socket.State.MaxLatency, out var reliable))
                return;

            var command = reliable.MenuSynchronizationCommand;
            if (!command.HasValue)
                return;

            switch (command.Value.Command)
            {
                case CharaSelectExit charaSelectExit:
                    Log.WriteLine($"[{nameof(CharacterSelect)}] Got Start/Exit Request Flag", LogCategory.Menu);
                    if (Socket.GetSocketType() == SocketType.Host)
                        Socket.SendToAllExceptAndFlush(packet.Source, new ReliablePacket(new CharaSelectExit(charaSelectExit.Type)), DeliveryMethod.ReliableOrdered);

                    _exit = charaSelectExit.Type;
                    Log.WriteLine($"[{nameof(CharacterSelect)}] Got Start/Exit Request Flag Complete", LogCategory.Menu);
                    break;
                case CharaSelectLoop charaSelectLoop:
                    var hostState = (HostState) Socket.State;
                    _loops[hostState.GetLocalPlayerIndex(packet.Source)] = charaSelectLoop;
                    break;

                case CharaSelectSync charaSelectSync:
                    _sync = new Volatile<Timestamped<CharaSelectSync>>(charaSelectSync);
                    break;
            }
        }

        /// <summary>
        /// Common implementation for syncing character select events.
        /// </summary>
        /// <param name="task">Current character select task.</param>
        private unsafe void Synchronize(Task<Sewer56.SonicRiders.Structures.Tasks.CharacterSelect, CharacterSelectTaskState>* task)
        {
            if (!_sync.HasValue)
                return;

            var result = _sync.Get();
            if (result.IsDiscard(Socket.State.MaxLatency))
                return;

            LastSync = result.Value;
            result.Value.ToGame(task);
        }

        /// <summary>
        /// Empties the character select queue and populates an array of character entries, indexed by player id.
        /// </summary>
        private CharaSelectLoop[] GetCharacterSelect()
        {
            var charLoops = new CharaSelectLoop[Constants.MaxNumberOfPlayers];
            for (var x = 0; x < _loops.Length; x++)
            {
                var charSelect = _loops[x];
                if (charSelect.IsDiscard(Socket.State.MaxLatency))
                    continue;

                charLoops[x] = charSelect;
            }

            return charLoops;
        }
    }
}
