using System;
using System.IO;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Misc.BitStream;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Internal;
using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.BitStream;
using Sewer56.BitStream.ByteStreams;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;
using static Riders.Tweakbox.Configs.PhysicsEditorConfig.Internal;
using Player = Sewer56.SonicRiders.API.Player;
namespace Riders.Tweakbox.Configs;

public unsafe class PhysicsEditorConfig : IConfiguration
{
    private static PhysicsEditorConfig _default = PhysicsEditorConfig.FromGame();

    /// <inheritdoc />
    public Action ConfigUpdated { get; set; }

    /// <summary>
    /// Internal data of the physics editor.
    /// </summary>
    public Internal Data = new Internal();

    /// <summary>
    /// Creates a <see cref="PhysicsEditorConfig"/> from the values present in game memory.
    /// </summary>
    public static PhysicsEditorConfig FromGame()
    {
        var config   = new PhysicsEditorConfig();
        ref var data = ref config.Data;

        data.Contents = EnumsNET.FlagEnums.GetAllFlags<PhysicsEditorContents>();
        data.RunningPhysics1 = *Player.RunPhysics;
        data.RunningPhysics2 = *Player.RunPhysics2;

        data.CharacterTypeStats = new CharacterTypeStats[Player.TypeStats.Count];
        Player.TypeStats.CopyTo(data.CharacterTypeStats, Player.TypeStats.Count);

        data.TurbulenceProperties = new TurbulenceProperties[Player.TurbulenceProperties.Count];
        Player.TurbulenceProperties.CopyTo(data.TurbulenceProperties, Player.TurbulenceProperties.Count);

        data.PanelProperties = Static.PanelProperties;
        data.DecelProperties = Static.DecelProperties;
        data.SpeedShoeProperties = Static.SpeedShoeProperties;
        return config;
    }

    public IConfiguration GetCurrent() => FromGame();
    public IConfiguration GetDefault() => _default;
    public unsafe byte[] ToBytes() => Data.ToBytes();
    public unsafe void FromBytes(Span<byte> bytes)
    {
        Data.FromBytes(bytes, out int bytesRead);
        ConfigUpdated?.Invoke();
    }

    public void Apply()
    {
        *Player.RunPhysics = Data.RunningPhysics1;
        *Player.RunPhysics2 = Data.RunningPhysics2;
        if (Data.CharacterTypeStats != null)
            Player.TypeStats.CopyFrom(Data.CharacterTypeStats, Data.CharacterTypeStats.Length);

        if (Data.TurbulenceProperties != null)
            Player.TurbulenceProperties.CopyFrom(Data.TurbulenceProperties, Data.TurbulenceProperties.Length);

        Static.PanelProperties = Data.PanelProperties;
        Static.DecelProperties = Data.DecelProperties;
        Static.SpeedShoeProperties = Data.SpeedShoeProperties;
    }

    /* Internal representation of this config. */
    public class Internal
    {
        /// <summary>
        /// The data stored inside this struct.
        /// </summary>
        public PhysicsEditorContents Contents;

        public RunningPhysics RunningPhysics1;
        public RunningPhysics2 RunningPhysics2;
        public CharacterTypeStats[] CharacterTypeStats;
        public TurbulenceProperties[] TurbulenceProperties;
        public DashPanelProperties PanelProperties = Static.PanelProperties;
        public DecelProperties DecelProperties = Static.DecelProperties;
        public SpeedShoeProperties SpeedShoeProperties = Static.SpeedShoeProperties;

        public byte[] ToBytes()
        {
            using var extendedMemoryStream = new ExtendedMemoryStream();
            var bitStream = new BitStream<StreamByteStream>(new StreamByteStream(extendedMemoryStream));

            bitStream.WriteGeneric(Contents);
            bitStream.WriteGeneric(ref RunningPhysics1);
            bitStream.WriteGeneric(ref RunningPhysics2);
            
            bitStream.WriteGeneric(CharacterTypeStats.AsSpan());
            bitStream.WriteGeneric(TurbulenceProperties.AsSpan());
            
            bitStream.WriteGeneric(ref PanelProperties);
            bitStream.WriteGeneric(ref DecelProperties);
            bitStream.WriteGeneric(ref SpeedShoeProperties);

            return extendedMemoryStream.ToArray();
        }

        public void FromBytes(Span<byte> bytes, out int numBytesRead)
        {
            fixed (byte* bytePtr = &bytes[0])
            {
                using var stream = new UnmanagedMemoryStream(bytePtr, bytes.Length);
                var bitStream = new BitStream<StreamByteStream>(new StreamByteStream(stream));

                bitStream.ReadGeneric(out Contents);
                bitStream.ReadIfHasFlags(ref RunningPhysics1, Contents, PhysicsEditorContents.Running);
                bitStream.ReadIfHasFlags(ref RunningPhysics2, Contents, PhysicsEditorContents.Running);
                ReadTypeStats(ref bitStream);
                bitStream.ReadIfHasFlags(ref TurbulenceProperties, Player.TurbulenceProperties.Count, Contents, PhysicsEditorContents.TurbulenceProperties);
                bitStream.ReadIfHasFlags(ref PanelProperties, Contents, PhysicsEditorContents.PanelAndDecelProperties);
                bitStream.ReadIfHasFlags(ref DecelProperties, Contents, PhysicsEditorContents.PanelAndDecelProperties);
                bitStream.ReadIfHasFlags(ref SpeedShoeProperties, Contents, PhysicsEditorContents.SpeedShoeProperties);
                numBytesRead = (int)bitStream.NextByteIndex;
            }
        }

        private void ReadTypeStats<TStreamType>(ref BitStream<TStreamType> bitStream) where TStreamType : IByteStream
        {
            // Ignore Obsolete Stats.
            CharacterTypeStats[] dummy = default;
            bitStream.ReadIfHasFlags(ref dummy, Player.TypeStats.Count, Contents, (PhysicsEditorContents)(1 << 1));

            // Read non-obsolete version.
            bitStream.ReadIfHasFlags(ref CharacterTypeStats, Player.TypeStats.Count, Contents, PhysicsEditorContents.TypeStatsFixed);
        }

        public enum PhysicsEditorContents : int
        {
            Running = 1 << 0,
            //TypeStats = 1 << 1, // Obsolete/Unused
            TurbulenceProperties = 1 << 2,
            PanelAndDecelProperties = 1 << 3,
            SpeedShoeProperties = 1 << 4,
            TypeStatsFixed = 1 << 5,
        }
    }
}
