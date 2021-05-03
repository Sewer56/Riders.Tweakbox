using System;
using LiteNetLib;
using Riders.Netplay.Messages.Helpers.Interfaces;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Riders.Tweakbox.Components.Common;
using Sewer56.Imgui.Controls;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayConfig : JsonConfigBase<NetplayConfig, NetplayConfig.Internal>
    {
        private const int TextLength = 128;
        private const int IPLength   = 15;

        /// <summary>
        /// Turns a player config into user data to send over the web.
        /// </summary>
        public PlayerData ToPlayerData()
        {
            return new PlayerData()
            {
                Name = Data.PlayerSettings.PlayerName.Text,
                NumPlayers = Data.PlayerSettings.LocalPlayers,
                PlayerIndex = 0,
            };
        }

        public class Internal
        {
            // Gap in keys due to removed old items.
            public SimulateBadInternet BadInternet  = new SimulateBadInternet();
            public NatPunchingServer PunchingServer = new NatPunchingServer();
            public HostSettings HostSettings        = new HostSettings();
            public ClientSettings ClientSettings    = new ClientSettings();
            public PlayerSettings PlayerSettings    = new PlayerSettings();
        }

        public class PlayerSettings
        {
            public TextInputData PlayerName = new TextInputData(Environment.UserName, TextLength);

            public int LocalPlayers = 1;
            public int MaxNumberOfCameras = 0;
            public JitterBufferSettings BufferSettings = new JitterBufferSettings();
        }

        public class ClientSettings
        {
            public SocketSettings SocketSettings = new SocketSettings();

            /// <summary>
            /// IP address of the host.
            /// </summary>
            public TextInputData IP = new TextInputData("127.0.0.1", IPLength);
        }

        public class HostSettings
        {
            public SocketSettings SocketSettings = new SocketSettings();
            public bool ReducedTickRate = false;
        }

        public class SocketSettings
        {
            public TextInputData Password = new TextInputData(String.Empty, TextLength);
            public int Port = 42069;
        }

        public class NatPunchingServer
        {
            public bool IsEnabled = true;
            public int Port = 6776;
            public TextInputData Host = new TextInputData("puncher.sewer56.moe", TextLength);
            public int ServerTimeout = 8000;
            public int PunchTimeout = 8000;
        }

        public class JitterBufferSettings
        {
            public JitterBufferType Type = JitterBufferType.Hybrid;
            public int MaxRampDownAmount = 10;
            public int DefaultBufferSize = 3; 
            public int NumJitterValuesSample = 180;
        }

        public class SimulateBadInternet
        {
            public bool IsEnabled;

            /// <summary>
            /// Packet loss between 0 and 100.
            /// </summary>
            public byte PacketLoss;

            /// <summary>
            /// Latency in ms to simulate.
            /// </summary>
            public byte MinLatency;

            /// <summary>
            /// Latency in ms to simulate.
            /// </summary>
            public byte MaxLatency;

            public void Apply(NetManager manager)
            {
                if (!IsEnabled)
                {
                    manager.SimulatePacketLoss = false;
                    manager.SimulateLatency = false;
                    return;
                }

                manager.SimulatePacketLoss = PacketLoss > 0 && PacketLoss <= 100;
                if (manager.SimulatePacketLoss)
                    manager.SimulationPacketLossChance = PacketLoss;

                manager.SimulateLatency = MinLatency > 0 && MaxLatency > MinLatency;
                if (manager.SimulateLatency)
                {
                    manager.SimulationMaxLatency = MaxLatency;
                    manager.SimulationMinLatency = MinLatency;
                }
            }
        }
    }
}
