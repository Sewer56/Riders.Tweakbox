using Riders.Tweakbox.Services.Interfaces;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Parser.Texture.Structs;
using System;
using static Sewer56.SonicRiders.Functions.Functions;
namespace Riders.Tweakbox.Services.TextureGen;

/// <summary>
/// Service which uses game code to convert PVRT textures to DDS.
/// </summary>
public class PvrtConverterService : ISingletonService
{
    /// <summary>
    /// Size of the header in PVRT texture data.
    /// </summary>
    public const int TextureHeaderSize = PvrtGeneratorService.TextureHeaderSize;

    private GetDdsTextureSizeForPvrtFn _getDdsSize = Functions.GetDdsTextureSizeForPvrt.GetWrapper();
    private ConvertToDdsFileFn _convertToDdsFile = Functions.ConvertToDdsFile.GetWrapper();

    /// <summary>
    /// Converts a set of textures to DDS.
    /// </summary>
    /// <param name="pvrtData">The PVRT textures to convert.</param>
    /// <returns>DDS data generated from PVRTs.</returns>
    public unsafe byte[][] ConvertMany(byte[][] pvrtData)
    {
        var result = GC.AllocateUninitializedArray<byte[]>(pvrtData.Length);
        for (int x = 0; x < pvrtData.Length; x++)
            result[x] = Convert(pvrtData[x]);

        return result;
    }

    /// <summary>
    /// Converts a texture to DDS.
    /// </summary>
    /// <param name="pvrtData">The PVRT texture to convert.</param>
    /// <returns>DDS data generated from PVRT.</returns>
    public unsafe byte[] Convert(Span<byte> pvrtData)
    {
        fixed (byte* pvrtDataPtr = pvrtData)
        {
            var header = (TextureHeader*)pvrtDataPtr;
            var ddsSize = _getDdsSize(header->Pvrt.Width, header->Pvrt.Height, header->Pvrt.DataFormat);
            var ddsData = GC.AllocateUninitializedArray<byte>(ddsSize);
            fixed (byte* ddsDataPtr = &ddsData[0])
            {
                _convertToDdsFile(ddsDataPtr, pvrtDataPtr + TextureHeaderSize, header->Pvrt.Width, header->Pvrt.Height, header->Pvrt.DataFormat);
                return ddsData;
            }
        }
    }
}
