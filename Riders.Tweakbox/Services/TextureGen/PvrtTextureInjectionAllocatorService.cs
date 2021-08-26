using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;
using Riders.Tweakbox.Services.TextureGen.Structs;

namespace Riders.Tweakbox.Services.TextureGen;

/// <summary>
/// Service which uses the remaining PVRT services to generate dummy textures, convert them and hash them
/// in order to return pools of texture hashes that can be used with the built in <see cref="Texture.TextureService"/>
/// for texture injection.
/// </summary>
public class PvrtTextureInjectionAllocatorService : ISingletonService
{
    private PvrtGeneratorService _generatorService = IoC.GetSingleton<PvrtGeneratorService>();
    private PvrtInjectionHasherService _hasherService = IoC.GetSingleton<PvrtInjectionHasherService>();

    /// <summary>
    /// Generates a set of unique textures and returns the corresponding hashes and metadata.
    /// </summary>
    /// <param name="count">The number of textures to allocate.</param>
    /// <param name="options">Options used for texture generation.</param>
    public PvrtTextureInjectionAllocation Allocate(int count, PvrtGeneratorSettings options)
    {
        var textures = _generatorService.GenerateMany(count, options, out ulong seed);
        var hashes = _hasherService.HashPvrtMany(textures);
        return new PvrtTextureInjectionAllocation(seed, options, hashes, _generatorService);
    }
}
