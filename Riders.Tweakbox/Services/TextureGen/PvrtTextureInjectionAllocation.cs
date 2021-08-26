using Riders.Tweakbox.Services.TextureGen.Structs;

namespace Riders.Tweakbox.Services.TextureGen;

/// <summary>
/// Represents a slice of hashes for dummy generated textures.
/// </summary>
public class PvrtTextureInjectionAllocation
{
    /// <summary>
    /// The seed of the first texture in this allocation.
    /// Can be reused with <see cref="PvrtGeneratorService"/> to obtain the source file.
    /// </summary>
    public ulong Seed { get; private set; }

    /// <summary>
    /// The hashes associated with this texture allocation.
    /// </summary>
    public string[] Hashes { get; private set; }

    /// <summary>
    /// The options used for generating this allocation.
    /// </summary>
    public PvrtGeneratorSettings Options { get; private set; }

    /// <summary>
    /// The amount of hashes associated with this allocation.
    /// </summary>
    public int Count => Hashes.Length;

    private PvrtGeneratorService _generatorService;

    private PvrtTextureInjectionAllocation() { }
    public PvrtTextureInjectionAllocation(ulong seed, PvrtGeneratorSettings options, string[] hashes, PvrtGeneratorService generatorService)
    {
        Seed = seed;
        Hashes = hashes;
        _generatorService = generatorService;
        Options = options;
    }

    public PvrtTextureInjectionAllocation(PvrtTextureInjectionAllocation existing)
    {
        Seed = existing.Seed;
        Hashes = existing.Hashes;
        _generatorService = existing._generatorService;
        Options = existing.Options;
    }

    /// <summary>
    /// Re-generates the original pvrt data for a given texture in this allocation.
    /// </summary>
    /// <param name="index">Zero based index of the texture in this allocation. i.e. 0 corresponds to <see cref="Hashes"/>[0]. </param>
    /// <returns>The PVRT data.</returns>
    public byte[] GeneratePvrt(uint index) => _generatorService.GetData(Options, Seed + index);
}
