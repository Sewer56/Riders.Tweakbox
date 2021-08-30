using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using Riders.Tweakbox.Services.Texture.Animation;
using SharpDX;
using SharpDX.Direct3D9;
using Riders.Tweakbox.Services.Texture.Enums;

namespace Riders.Tweakbox.Services.Texture;

/// <summary>
/// Provides tools for working with animated textures.
/// </summary>
public class AnimatedTextureCacheTools
{
    private const int MinFilesBeforeArchiveCache = 10;

    /// <summary>
    /// Returns true if a list of textures should be cached; else false.
    /// </summary>
    public static bool ShouldCache(int fileCount) => fileCount >= MinFilesBeforeArchiveCache;

    /// <summary>
    /// Creates an archive from a given list of DDS files and saves it to a given directory.
    /// </summary>
    /// <param name="cacheFiles">The texture files to include in the archive.</param>
    /// <param name="cachePath">The path to save the cache to.</param>
    public static void CreateArchive(List<byte[]> cacheFiles, string cachePath)
    {
        using var cacheWriter = new AnimatedTextureCacheWriter((cacheFiles[0].Length * cacheFiles.Count) + 16, cacheFiles.Count);
        foreach (var file in cacheFiles)
            cacheWriter.TryWriteFile(file);

        cacheWriter.Finish();
        Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
        TextureCompression.PickleToFile(cachePath, cacheWriter.GetSpan(), LZ4Level.L11_OPT);
    }

    /// <summary>
    /// Queues a task to the threadpool which will create archive from a given list of DDS files and save it to a given path.
    /// </summary>
    /// <param name="cacheFiles">The texture files to include in the archive.</param>
    /// <param name="cachePath">The path to save the cache to.</param>
    public static void QueueCreateArchive(List<byte[]> cacheFiles, string cachePath) => ThreadPool.QueueUserWorkItem((uwu) => CreateArchive(cacheFiles, cachePath));

    /// <summary>
    /// Tries to load textures from a specific cache file.
    /// </summary>
    /// <param name="device">The device to load the texture info.</param>
    /// <param name="cachePath">Path to the texture cache file.</param>
    /// <param name="totalFileCount">The total amount of expected textures to be loaded. This should be total number of textures - 1.</param>
    /// <param name="textures">The array of textures to receive the loaded texture data.</param>
    /// <param name="firstTextureOffset">Offset into the array to place first texture into.</param>
    /// <param name="token">Token used to cancel the operation.</param>
    /// <returns></returns>
    public static unsafe bool TryLoadFromCacheFile(Device device, string cachePath, int totalFileCount, SharpDX.Direct3D9.Texture[] textures, int firstTextureOffset = 1, CancellationToken token = default)
    {
        var data = TextureCompression.PickleFromFile(cachePath);
        using var reader = new AnimatedTextureCacheReader(data);
        
        int count = firstTextureOffset;
        while (reader.TryGetNextFile(out int size, out byte* dataPtr))
        {
            var unmangedStream = new DataStream((IntPtr)dataPtr, size, true, true);
            textures[count++] = SharpDX.Direct3D9.Texture.FromStream(device, unmangedStream, Usage.None, Pool.Default);

            if (token.IsCancellationRequested)
                return true;
        }

        return true;
    }

    /// <summary>
    /// Returns true if an animated texture should be loaded from cache; else false.
    /// </summary>
    /// <param name="cachePath">The path to the cache file.</param>
    /// <param name="newestFileTime">The file time of the latest file in the original folder.</param>
    public static bool CanLoadFromCache(string cachePath, DateTime newestFileTime)
    {
        try
        {
            return File.GetLastWriteTimeUtc(cachePath) > newestFileTime;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns true if a warning should be given for a given animated texture.
    /// </summary>
    /// <param name="fileCount">Number of files.</param>
    /// <param name="texFormat">Texture format.</param>
    public static bool ShouldGivePerfWarning(int fileCount, TextureFormat texFormat) => fileCount > MinFilesBeforeArchiveCache && texFormat == TextureFormat.Png;
}
