using System;
using Reloaded.Memory;
using Reloaded.Memory.Streams;
using Riders.Tweakbox.Definitions.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;
using Player = Sewer56.SonicRiders.API.Player;

namespace Riders.Tweakbox.Components.PhysicsEditor
{
    public unsafe class PhysicsEditorConfig : IConfiguration
    {
        private static PhysicsEditorConfig _default = PhysicsEditorConfig.FromGame();

        public struct Internal
        {
            public RunningPhysics  RunningPhysics1 { get; set; }
            public RunningPhysics2 RunningPhysics2 { get; set; }
        }

        /// <summary>
        /// Internal data of the physics editor.
        /// </summary>
        public Internal Data
        {
            get => _data;
            set => _data = value;
        }

        private Internal _data;

        /// <summary>
        /// Creates a <see cref="PhysicsEditorConfig"/> from the values present in game memory.
        /// </summary>
        public static PhysicsEditorConfig FromGame()
        {
            return new PhysicsEditorConfig
            {
                _data =
                {
                    RunningPhysics1 = *Player.RunPhysics,
                    RunningPhysics2 = *Player.RunPhysics2
                }
            };
        }

        public unsafe byte[] ToBytes()
        {
            using var reloadedMemoryStream = new ExtendedMemoryStream(new byte[sizeof(RunningPhysics) + sizeof(RunningPhysics2)]);
            reloadedMemoryStream.Write(Data);
            return reloadedMemoryStream.ToArray();
        }

        public unsafe Span<byte> FromBytes(Span<byte> bytes)
        {
            Struct.FromArray(bytes, out _data);
            return bytes.Slice(sizeof(RunningPhysics) + sizeof(RunningPhysics2));
        }

        public void Apply()
        {
            *Player.RunPhysics  = _data.RunningPhysics1;
            *Player.RunPhysics2 = _data.RunningPhysics2;
        }

        public IConfiguration GetDefault() => _default;
    }
}
