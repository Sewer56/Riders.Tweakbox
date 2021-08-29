using System;
using System.Linq;
using System.Runtime.InteropServices;
using DotNext.Buffers;
using K4os.Compression.LZ4;
using Reloaded.Memory;
using Riders.Netplay.Messages.Misc.BitStream;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Riders.Tweakbox.Shared;
using Riders.Tweakbox.Shared.Structs;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.API.Player;
namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay;

[StructLayout(LayoutKind.Sequential)]
[Equals(DoNotAddEqualityOperators = true)]
public unsafe struct GameData : IReliableMessage
{
    public static readonly int NumTurbulenceProperties = Player.TurbulenceProperties.Count;

    /// <summary>
    /// Extreme gear information.
    /// </summary>
    public GearData GearData;

    /// <summary>
    /// Contains all of the properties regarding how turbulence is internally handled.
    /// </summary>
    public TurbulenceProperties[] TurbulenceProperties;

    /// <summary>
    /// Contains the running physics for this instance.
    /// </summary>
    public RunningPhysics RunningPhysics1;

    /// <summary>
    /// Contains the running physics (struct 2) for this instance.
    /// </summary>
    public RunningPhysics2 RunningPhysics2;

    /// <summary>
    /// The current settings for the race.
    /// </summary>
    public RaceSettings RaceSettings;

    /// <summary>
    /// Current Dash Panel Properties.
    /// </summary>
    public DashPanelProperties PanelProperties;

    /// <summary>
    /// Current deceleration properties.
    /// </summary>
    public DecelProperties DecelProperties;

    /// <summary>
    /// Current deceleration properties.
    /// </summary>
    public SpeedShoeProperties SpeedShoeProperties;

    /// <inheritdoc />
    public void Dispose() { }

    /// <summary>
    /// Retrieves the expected size of data to be sent over the network.
    /// </summary>
    public int GetDataSize()
    {
        // This struct
        var size = StructArray.GetSize<TurbulenceProperties>(Player.TurbulenceProperties.Count) +
                   sizeof(RunningPhysics) + sizeof(RunningPhysics2) + sizeof(RaceSettings) + sizeof(DashPanelProperties)
                   + sizeof(DecelProperties) + sizeof(SpeedShoeProperties);

        // Children
        size += GearData.GetDataSize();
        return size + 1;
    }

    /// <summary>
    /// Writes the contents of this packet to the game memory.
    /// </summary>
    /// <param name="applyCustomGears">A function which applies custom gears to the game. Returns true on success, else false.</param>
    public unsafe void ToGame(Func<string[], bool> applyCustomGears)
    {
        // TODO: Custom Gear Support
        if (GearData.ToGame(applyCustomGears))
        {
            *Player.RunPhysics = RunningPhysics1;
            *Player.RunPhysics2 = RunningPhysics2;
            *State.CurrentRaceSettings = RaceSettings;
            Player.TurbulenceProperties.CopyFrom(TurbulenceProperties, TurbulenceProperties.Length);
            Static.PanelProperties = PanelProperties;
            Static.SpeedShoeProperties = SpeedShoeProperties;
            Static.DecelProperties.Value = DecelProperties;
        }
    }

    /// <summary>
    /// Retrieves gear information from the game.
    /// </summary>
    /// <param name="customGears">List of custom gear names.</param>
    public static unsafe GameData FromGame(string[] customGears)
    {
        var data = new GameData
        {
            RunningPhysics1 = *Player.RunPhysics,
            RunningPhysics2 = *Player.RunPhysics2,
            RaceSettings = *State.CurrentRaceSettings,
            GearData = GearData.FromGame(customGears),
            TurbulenceProperties = Player.TurbulenceProperties.ToArray(),
            PanelProperties = Static.PanelProperties,
            SpeedShoeProperties = Static.SpeedShoeProperties,
            DecelProperties = Static.DecelProperties.Value
        };

        return data;
    }

    /// <summary>
    /// Returns a rented byte array corresponding to this structure.
    /// </summary>
    public ArrayRental<byte> ToCompressedBytes(out int bytesWritten)
    {
        var rental = new ArrayRental<byte>(LZ4Codec.MaximumOutputSize(GetDataSize()));
        var rentalStream = new RentalByteStream(rental);
        var bitStream = new BitStream<RentalByteStream>(rentalStream);
        ToStream(ref bitStream);

        bytesWritten = bitStream.NextByteIndex;
        return rental;
    }

    /// <inheritdoc />
    public readonly MessageType GetMessageType() => MessageType.GameData;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        // Get data lengths
        var numCompressedBytes = bitStream.Read<int>();
        var numDecompressedBytes = bitStream.Read<int>();

        // Decompress.
        using var compressedRental = new ArrayRental<byte>(numCompressedBytes);
        using var uncompressedRental = new ArrayRental<byte>(numDecompressedBytes);
        var compressedSpan = compressedRental.Span.Slice(0, numCompressedBytes);
        var decompressedSpan = uncompressedRental.Span.Slice(0, numDecompressedBytes);

        bitStream.Read(compressedSpan);
        LZ4Codec.Decode(compressedSpan, decompressedSpan);

        // Read
        FromBytes(uncompressedRental);
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        // Compress
        using var uncompressedBytes = ToBytes(out int numUncompressedBytes);
        using var compressedBytes = new ArrayRental<byte>(LZ4Codec.MaximumOutputSize(numUncompressedBytes));
        int numCompressedBytes = LZ4Codec.Encode(uncompressedBytes.Span.Slice(0, numUncompressedBytes), compressedBytes.Span, LZ4Level.L12_MAX);

        // Write compressed & decompressed data lengths.
        bitStream.Write<int>(numCompressedBytes);
        bitStream.Write<int>(numUncompressedBytes);

        // Write compressed.
        bitStream.Write(compressedBytes.Span.Slice(0, numCompressedBytes));
    }

    private void FromBytes(ArrayRental<byte> rental)
    {
        var stream = new RentalByteStream(rental);
        var bitStream = new BitStream<RentalByteStream>(stream);

        GearData.FromStream(ref bitStream);
        TurbulenceProperties = new TurbulenceProperties[NumTurbulenceProperties];
        RunningPhysics1 = bitStream.ReadGeneric<RunningPhysics>();
        RunningPhysics2 = bitStream.ReadGeneric<RunningPhysics2>();
        RaceSettings = bitStream.ReadGeneric<RaceSettings>();
        
        for (int x = 0; x < NumTurbulenceProperties; x++)
            TurbulenceProperties[x] = bitStream.ReadGeneric<TurbulenceProperties>();

        PanelProperties = bitStream.ReadGeneric<DashPanelProperties>();
        DecelProperties = bitStream.ReadGeneric<DecelProperties>();
        SpeedShoeProperties = bitStream.ReadGeneric<SpeedShoeProperties>();
    }

    private ArrayRental<byte> ToBytes(out int bytesWritten)
    {
        var rental = new ArrayRental<byte>(GetDataSize());
        var bitStream = new BitStream<RentalByteStream>(new RentalByteStream(rental));

        GearData.ToStream(ref bitStream);
        bitStream.WriteGeneric(RunningPhysics1);
        bitStream.WriteGeneric(RunningPhysics2);
        bitStream.WriteGeneric(RaceSettings);

        for (int x = 0; x < NumTurbulenceProperties; x++)
            bitStream.WriteGeneric(TurbulenceProperties[x]);

        bitStream.WriteGeneric(PanelProperties);
        bitStream.WriteGeneric(DecelProperties);
        bitStream.WriteGeneric(SpeedShoeProperties);
        bytesWritten = bitStream.NextByteIndex;
        return rental;
    }
}
