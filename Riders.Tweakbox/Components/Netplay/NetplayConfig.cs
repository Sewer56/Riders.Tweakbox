using System;
using System.Text;
using System.Text.Json;
using MessagePack;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Definitions.Serializers.Json;
using Riders.Tweakbox.Definitions.Serializers.MessagePack;
using Sewer56.Imgui.Controls;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayConfig : IConfiguration
    {
        private static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            IncludeFields = true,
            WriteIndented = true,
            Converters = { new TextInputJsonConverter() }
        };

        private const int TextLength = 128;
        private const int IPLength   = 15;

        public Internal Data = new Internal();
        
        /// <inheritdoc />
        public Action ConfigUpdated { get; set; }

        /// <inheritdoc />
        public byte[] ToBytes() => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Data, SerializerOptions));

        /// <inheritdoc />
        public unsafe void FromBytes(Span<byte> bytes)
        {
            Data = JsonSerializer.Deserialize<Internal>(bytes, SerializerOptions);
            ConfigUpdated?.Invoke();
        }

        /// <inheritdoc />
        public void Apply() { }

        /// <inheritdoc />
        public IConfiguration GetCurrent() => this;

        /// <inheritdoc />
        public IConfiguration GetDefault() => new NetplayConfig();

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
            public SimulateBadInternet BadInternet = new SimulateBadInternet();
            public NatPunchingServer PunchingServer = new NatPunchingServer();
            public HostSettings HostSettings = new HostSettings();
            public ClientSettings ClientSettings = new ClientSettings();
            public PlayerSettings PlayerSettings = new PlayerSettings();
        }

        public class PlayerSettings
        {
            public TextInputData PlayerName = new TextInputData(Environment.UserName, TextLength);

            public int LocalPlayers = 1;
            public int MaxNumberOfCameras = 0;
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
        }

        public class SimulateBadInternet
        {
            [Key(0)]
            public bool IsEnabled;

            /// <summary>
            /// Packet loss between 0 and 100.
            /// </summary>
            [Key(1)]
            public byte PacketLoss;

            /// <summary>
            /// Latency in ms to simulate.
            /// </summary>
            [Key(2)]
            public byte MinLatency;

            /// <summary>
            /// Latency in ms to simulate.
            /// </summary>
            [Key(3)]
            public byte MaxLatency;
        }
    }
}
