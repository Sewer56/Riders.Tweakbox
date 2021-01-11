using System;
using System.Diagnostics;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    /// <inheritdoc />
    public unsafe class Client : Socket
    {
        public Client(string ipAddress, int port, string password, NetplayController controller) : base(controller)
        {
            Log.WriteLine($"[Client] Joining Server on {ipAddress}:{port} with password {password}", LogCategory.Socket);
            if (Event.LastTask != Tasks.CourseSelect)
                throw new Exception("You are only allowed to join the host in the Course Select Menu");

            Manager.StartInManualMode(0);
            State = new CommonState(IoC.GetConstant<NetplayImguiConfig>().ToHostPlayerData());
            Manager.Connect(ipAddress, port, password);
            Initialize();
        }

        public override void Dispose()
        {
            Log.WriteLine($"[Client] Disposing of Socket, Disconnected", LogCategory.Socket);
            base.Dispose();
        }

        public override SocketType GetSocketType() => SocketType.Client;

        public override void HandlePacket(Packet<NetPeer> packet)
        {
            if (packet.GetPacketKind() == PacketKind.Reliable)
                HandleReliable(packet.As<ReliablePacket>());
        }

        private void HandleReliable(ReliablePacket packet)
        {
            if (packet.ServerMessage.HasValue)
                HandleServerMessage(packet.ServerMessage.Value);

            if (packet.GameData.HasValue)
            {
                Log.WriteLine($"[Client] Received Game Data, Applying", LogCategory.Socket);
                packet.GameData.Value.ToGame();
            }
        }

        private void HandleServerMessage(ServerMessage serverMessage)
        {
            var msg = serverMessage.Message;
            switch (msg)
            {
                case HostSetPlayerData hostSetPlayerData:
                    Log.WriteLine($"[Client] Received Player Info", LogCategory.Socket);
                    State.PlayerInfo = hostSetPlayerData.Data;
                    State.SelfInfo.PlayerIndex = hostSetPlayerData.Index;
                    break;

                case SetAntiCheat setAntiCheat:
                    Log.WriteLine($"[Client] Received Anticheat Info", LogCategory.Socket);
                    State.AntiCheatMode = setAntiCheat.Cheats;
                    break;
            }
        }

        #region Overrides
        public override void OnPeerConnected(NetPeer peer) => SendAndFlush(peer, new ReliablePacket(new ClientSetPlayerData(State.SelfInfo)), DeliveryMethod.ReliableUnordered, "[Client] Connected to Host, Sending Player Data", LogCategory.Socket);
        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => Dispose();

        // Ignored
        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public override bool OnConnectionRequest(ConnectionRequest request) { return true; }
        #endregion
    }
}
