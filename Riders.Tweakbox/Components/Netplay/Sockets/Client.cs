using System;
using System.Diagnostics;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Unreliable;
using Riders.Tweakbox.Components.Netplay.Components;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;
using Debug = System.Diagnostics.Debug;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    /// <inheritdoc />
    public unsafe class Client : Socket
    {
        public Client(string ipAddress, int port, string password, NetplayController controller) : base(controller)
        {
            Trace.WriteLine($"[Client] Joining Server on {ipAddress}:{port} with password {password}");
            if (Event.LastTask != Tasks.CourseSelect)
                throw new Exception("You are only allowed to join the host in the Course Select Menu");

            Manager.Start();
            State = new CommonState(IoC.GetConstant<NetplayImguiConfig>().ToHostPlayerData());

            // Add undo menu movement when connected.
            Event.OnSetSpawnLocationsStartOfRace += State.OnSetSpawnLocationsStartOfRace;
            Event.AfterSetSpawnLocationsStartOfRace += State.OnSetSpawnLocationsStartOfRace;

            Event.OnRace += OnRace;
            Event.AfterRace += AfterRace;
            Event.AfterSetMovementFlagsOnInput += OnAfterSetMovementFlagsOnInput;
            Event.OnCheckIfPlayerIsHuman += State.OnCheckIfPlayerIsHuman;
            Event.OnCheckIfPlayerIsHumanIndicator += State.OnCheckIfPlayerIsHuman;

            Manager.Connect(ipAddress, port, password);
            Initialize();
        }

        public override void Dispose()
        {
            Trace.WriteLine($"[Client] Disposing of Socket, Disconnected");
            base.Dispose();

            Event.OnSetSpawnLocationsStartOfRace -= State.OnSetSpawnLocationsStartOfRace;
            Event.AfterSetSpawnLocationsStartOfRace -= State.OnSetSpawnLocationsStartOfRace;

            Event.OnRace -= OnRace;
            Event.AfterRace -= AfterRace;
            Event.AfterSetMovementFlagsOnInput -= OnAfterSetMovementFlagsOnInput;
            Event.OnCheckIfPlayerIsHuman -= State.OnCheckIfPlayerIsHuman;
            Event.OnCheckIfPlayerIsHumanIndicator -= State.OnCheckIfPlayerIsHuman;
        }

        public override SocketType GetSocketType() => SocketType.Client;

        public override void HandlePacket(Packet<NetPeer> packet)
        {
            if (packet.As<IPacket>().GetPacketType() == PacketKind.Reliable)
                HandleReliable(packet.As<ReliablePacket>());

            else if (packet.As<IPacket>().GetPacketType() == PacketKind.Unreliable)
                HandleUnreliable(packet.As<UnreliablePacket>());
        }

        private void HandleUnreliable(UnreliablePacket result)
        {
            var players = result.Players;

            // Fill in from player 2.
            for (int x = 0; x < players.Length; x++)
                State.RaceSync[x + 1] = players[x];
        }

        private void HandleReliable(ReliablePacket packet)
        {
            if (packet.ServerMessage.HasValue)
                HandleServerMessage(packet.ServerMessage.Value);

            if (packet.GameData.HasValue)
            {
                Trace.WriteLine($"[Client] Received Game Data, Applying");
                packet.GameData.Value.ToGame();
            }

            if (packet.MovementFlags.HasValue)
            {
                packet.MovementFlags.Value.AsInterface().ToArray(State.MovementFlagsSync, MovementFlagsPacked.NumberOfEntries - 1, 0, 1);
            }
        }


        private void HandleServerMessage(ServerMessage serverMessage)
        {
            var msg = serverMessage.Message;
            switch (msg)
            {
                case HostSetPlayerData hostSetPlayerData:
                    Trace.WriteLine($"[Client] Received Player Info");
                    State.PlayerInfo = hostSetPlayerData.Data;
                    State.SelfInfo.PlayerIndex = hostSetPlayerData.Index;
                    break;

                case SetAntiCheat setAntiCheat:
                    Trace.WriteLine($"[Client] Received Anticheat Info");
                    State.AntiCheatMode = setAntiCheat.Cheats;
                    break;
            }
        }

        #region Events: On/After Events
        private void OnRace(Task<byte, RaceTaskState>* task)
        {
            Poll();
            State.ApplyRaceSync();
        }

        private void AfterRace(Task<byte, RaceTaskState>* task)
        {
            var packet = new UnreliablePacket(UnreliablePacketPlayer.FromGame(0, State.FrameCounter));
            SendAndFlush(Manager.FirstPeer, packet, DeliveryMethod.Sequenced);
        }

        private Player* OnAfterSetMovementFlagsOnInput(Player* player)
        {
            State.OnAfterSetMovementFlags(player);

            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (index == 0) 
                SendAndFlush(Manager.FirstPeer, new ReliablePacket() { SetMovementFlags = new MovementFlagsMsg(player) }, DeliveryMethod.ReliableOrdered);

            return player;
        }
        #endregion

        #region Overrides
        public override void OnPeerConnected(NetPeer peer) => SendAndFlush(peer, new ReliablePacket(new ClientSetPlayerData(State.SelfInfo)), DeliveryMethod.ReliableUnordered, "[Client] Connected to Host, Sending Player Data");
        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => Dispose();

        // Ignored
        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public override bool OnConnectionRequest(ConnectionRequest request) { return true; }
        #endregion
    }
}
