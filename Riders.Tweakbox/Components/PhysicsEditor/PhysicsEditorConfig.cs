using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Reloaded.Memory;
using Reloaded.Memory.Streams;
using Riders.Tweakbox.Components.GearEditor;
using Riders.Tweakbox.Definitions.Interfaces;
using Sewer56.SonicRiders.Fields;
using Sewer56.SonicRiders.Structures.Gameplay;

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
                    RunningPhysics1 = *Physics.RunningPhysics1,
                    RunningPhysics2 = *Physics.RunningPhysics2
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
            *Physics.RunningPhysics1 = _data.RunningPhysics1;
            *Physics.RunningPhysics2 = _data.RunningPhysics2;
        }

        public IConfiguration GetDefault() => _default;
    }
}
