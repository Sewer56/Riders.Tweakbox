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

        /// <summary>
        /// Internal data of the physics editor.
        /// </summary>
        public Internal Data;

        /// <summary>
        /// Creates a <see cref="PhysicsEditorConfig"/> from the values present in game memory.
        /// </summary>
        public static PhysicsEditorConfig FromGame()
        {
            return new PhysicsEditorConfig
            {
                Data =
                {
                    RunningPhysics1 = *Player.RunPhysics,
                    RunningPhysics2 = *Player.RunPhysics2
                }
            };
        }

        public unsafe byte[] ToBytes()
        {
            using var reloadedMemoryStream = new ExtendedMemoryStream(new byte[sizeof(Internal)]);
            reloadedMemoryStream.Write(Data);
            return reloadedMemoryStream.ToArray();
        }

        public unsafe Span<byte> FromBytes(Span<byte> bytes)
        {
            Struct.FromArray(bytes, out Data);
            return bytes.Slice(sizeof(Internal));
        }

        public void Apply()
        {
            *Player.RunPhysics  = Data.RunningPhysics1;
            *Player.RunPhysics2 = Data.RunningPhysics2;
        }

        public IConfiguration GetCurrent() => FromGame();
        public IConfiguration GetDefault() => _default;

        /* Internal representation of this config. */
        public struct Internal
        {
            public RunningPhysics RunningPhysics1;
            public RunningPhysics2 RunningPhysics2;
        }
    }
}
