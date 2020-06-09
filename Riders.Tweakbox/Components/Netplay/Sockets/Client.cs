using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LiteNetLib;
using Reloaded.Imgui.Hook.Misc;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets.Components;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    /// <inheritdoc />
    public class Client : Socket
    {
        public TaskTracker Tracker { get; private set; } = IoC.GetConstant<TaskTracker>();
        public ConcurrentQueue<Packet> PacketQueue { get; private set; } = new ConcurrentQueue<Packet>();
        public ClientState State { get; private set; } = new ClientState();

        public Client(string ipAddress, int port, string password)
        {
            Debug.WriteLine($"[Tweakbox] Joining Server on {ipAddress}:{port} with password {password}");

            // TODO: Implement Connection
            Manager.Start();
            Manager.Connect(ipAddress, port, password);
        }

        public override bool IsHost() => false;
        public override void Update()
        {
            while (PacketQueue.TryDequeue(out var result))
            {
                if (result.Reliable.HasValue)
                    UpdateReliable(result.Reliable.Value);
                if (result.Unreliable.HasValue)
                    UpdateUnreliable(result.Unreliable.Value);
            }
        }

        private void UpdateUnreliable(UnreliablePacket resultUnreliable)
        {

        }

        private void UpdateReliable(ReliablePacket resultReliable)
        {
            if (resultReliable.MenuSynchronizationCommand.HasValue)
                UpdateMenu(resultReliable.MenuSynchronizationCommand.Value);
        }

        private unsafe void UpdateMenu(MenuSynchronizationCommand syncCommand)
        {
            var cmd = syncCommand.Command;
            switch (cmd)
            {
                case CharaSelectSync charaSelectSync:
                    Debug.WriteLine($"[Client] Applying Character Sync");
                    charaSelectSync.ToGame(Tracker.CharacterSelect);
                    break;
                case CourseSelectSync courseSelectSync:
                    Debug.WriteLine($"[Client] Applying Course Select Sync");
                    courseSelectSync.ToGame(Tracker.CourseSelect);
                    break;
                case RuleSettingsSync ruleSettingsSync:
                    Debug.WriteLine($"[Client] Applying Rule Settings Sync");
                    ruleSettingsSync.ToGame(Tracker.RaceRules);
                    break;
            }
        }

        private void HandleServerMessage(NetPeer peer, ServerMessage serverMessage)
        {
            var msg = serverMessage.Message;
            switch (msg)
            {
                case HostSetPlayerData hostSetPlayerData:
                    Debug.WriteLine($"[Client] Received Player Info");
                    State.PlayerInfo = hostSetPlayerData.Data;
                    break;
                
                case SetAntiCheat setAntiCheat:
                    Debug.WriteLine($"[Client] Received Anticheat Info");
                    State.AntiCheatMode = setAntiCheat.Cheats;
                    break;
            }
        }

        #region Overrides / Events
        public override void HandleReliablePacket(NetPeer peer, ReliablePacket packet)
        {
            // Do not queue: Server Sync Command
            if (packet.ServerMessage != null)
            {
                HandleServerMessage(peer, packet.ServerMessage.Value);
                return;
            }

            PacketQueue.Enqueue(new Packet(packet, null));
        }

        public override void HandleUnreliablePacket(NetPeer peer, UnreliablePacket packet)
        {
            // TODO: [Client] Race / Handle Unreliable Packets
            PacketQueue.Enqueue(new Packet(null, packet));
        }

        public override void OnPeerConnected(NetPeer peer)
        {
            // Inform host of player data.
            var playerData = IoC.GetConstant<NetplayImguiConfig>();
            var setPlayerData = new ClientSetPlayerData() { Data = playerData.ToHostPlayerData() };
            var message = new ServerMessage(setPlayerData);
            var packet = new ReliablePacket { ServerMessage = message };
            peer.Send(packet.Serialize(), DeliveryMethod.ReliableUnordered);
            peer.Flush();
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => Dispose();

        // Ignored
        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public override void OnConnectionRequest(ConnectionRequest request) { }
        #endregion
    }
}
