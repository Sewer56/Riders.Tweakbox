using System;
using MessagePack;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Definitions.Serializers;
using Sewer56.Imgui.Controls;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayConfig : IConfiguration
    {
        private static IFormatterResolver MsgPackResolver = MessagePack.Resolvers.CompositeResolver.Create(NullableResolver.Instance, MessagePack.Resolvers.StandardResolver.Instance);
        private static MessagePackSerializerOptions SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(MsgPackResolver);

        private const int TextLength = 32;
        private const int IPLength   = 15;

        public Internal Data = Internal.Create();
        
        /// <inheritdoc />
        public Action ConfigUpdated { get; set; }

        /// <inheritdoc />
        public byte[] ToBytes() => MessagePackSerializer.Serialize(Data, SerializerOptions);

        /// <inheritdoc />
        public unsafe Span<byte> FromBytes(Span<byte> bytes)
        {
            Data = Utilities.DeserializeMessagePack<Internal>(bytes, out int numBytesRead, SerializerOptions);
            Data.Initialize();
            ConfigUpdated?.Invoke();
            return bytes.Slice((int)numBytesRead);
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
                Name = Data.PlayerName.Text,
                NumPlayers = Data.LocalPlayers.Value,
                PlayerIndex = 0,
            };
        }

        [MessagePackObject]
        public struct Internal
        {
            [Key(0)]
            [MessagePackFormatter(typeof(TextInputDataFormatter))]
            public TextInputData PlayerName;

            [Key(1)]
            [MessagePackFormatter(typeof(TextInputDataFormatter))]
            public TextInputData Password;

            [Key(2)]
            [MessagePackFormatter(typeof(TextInputDataFormatter))]
            public TextInputData ClientIP;

            [Key(3)]
            public Definitions.Nullable<int> HostPort;

            [Key(4)]
            public Definitions.Nullable<int> ClientPort;

            [Key(5)]
            public Definitions.Nullable<bool> ShowPlayers;

            [Key(6)]
            public SimulateBadInternet BadInternet;

            [Key(7)]
            public Definitions.Nullable<bool> ReducedTickRate;

            [Key(8)]
            public Definitions.Nullable<int> LocalPlayers;

            [Key(9)]
            public Definitions.Nullable<int> MaxNumberOfCameras;

            public static Internal Create()
            {
                var value = new Internal();
                value.Initialize();
                return value;
            }

            public void Initialize()
            {
                PlayerName ??= new TextInputData("Pixel", TextLength);
                Password ??= new TextInputData(String.Empty, TextLength);
                ClientIP ??= new TextInputData("127.0.0.1", IPLength);
                HostPort.SetIfNull(42069);
                ClientPort.SetIfNull(42069);
                ShowPlayers.SetIfNull(false);
                ReducedTickRate.SetIfNull(false);
                LocalPlayers.SetIfNull(1);
                MaxNumberOfCameras.SetIfNull(0);
            }

            [MessagePackObject]
            public struct SimulateBadInternet
            {
                /// <summary>
                /// True if enabled.
                /// </summary>
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
}
