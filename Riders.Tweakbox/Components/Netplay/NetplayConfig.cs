using System;
using System.IO;
using MessagePack;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Definitions.Serializers;
using Sewer56.Imgui.Controls;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayConfig : IConfiguration
    {
        private const int TextLength = 32;
        private const int IPLength   = 15;

        public Internal Data = Internal.Create();
        
        /// <summary>
        /// Turns a player config into user data to send over the web.
        /// </summary>
        public HostPlayerData ToHostPlayerData()
        {
            return new HostPlayerData()
            {
                Name = Data.PlayerName.Text,
                PlayerIndex = 0
            };
        }

        /// <inheritdoc />
        public byte[] ToBytes() { return MessagePackSerializer.Serialize(Data); }

        /// <inheritdoc />
        public unsafe Span<byte> FromBytes(Span<byte> bytes)
        {
            fixed (byte* ptr = bytes)
            {
                using var stream = new UnmanagedMemoryStream(ptr, bytes.Length);
                var initialOffset = stream.Position;
                Data = MessagePackSerializer.Deserialize<Internal>(stream);
                var bytesRead = stream.Position - initialOffset;
                return bytes.Slice((int) bytesRead);
            }
        }

        /// <inheritdoc />
        public void Apply() { }

        /// <inheritdoc />
        public IConfiguration GetCurrent() => this;

        /// <inheritdoc />
        public IConfiguration GetDefault() => new NetplayConfig();

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
            public int HostPort;

            [Key(4)]
            public int ClientPort;

            [Key(5)]
            public bool ShowPlayers;

            public static Internal Create()
            {
                return new Internal
                {
                    PlayerName = new TextInputData("Pixel", TextLength),
                    Password = new TextInputData(String.Empty, TextLength),
                    ClientIP = new TextInputData("127.0.0.1", IPLength),
                    HostPort = 42069,
                    ClientPort = 42069,
                    ShowPlayers = false
                };
            }
        }
    }
}
