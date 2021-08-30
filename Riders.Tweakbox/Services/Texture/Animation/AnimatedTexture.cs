using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Data;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Services.Texture.Animation;
using Riders.Tweakbox.Services.Texture.Enums;
using Riders.Tweakbox.Services.Texture.Structs;
using SharpDX;
using SharpDX.Direct3D9;
namespace Riders.Tweakbox.Services.Texture.Animation;

public class AnimatedTexture : IDisposable
{
    private const int MinFilesBeforeArchiveCache = 10;

    /// <summary>
    /// The folder where the texture set resides.
    /// </summary>
    public string Folder { get; private set; }

    /// <summary>
    /// List of files associated with the folder.
    /// </summary>
    public List<AnimatedTextureFile> Files { get; private set; }

    /// <summary>
    /// Individual loaded in textures.
    /// </summary>
    public List<SharpDX.Direct3D9.Texture> Textures { get; private set; }

    /// <summary>
    /// Path to the cache file for this animated texture.
    /// </summary>
    public string CachePath => Path.Combine(Folder, "Cache", "cache.bin");

    private int _modulo = 0;
    private List<int> _minId;
    private bool _loaded;
    private int _lastIndex;

    // Cache Related Parameters
    private DateTime _newestFileTime;
    private CancellationTokenSource _preloadFromCacheToken;
    private Task _preloadFromCacheTask;

    // Misc
    private static Logger _log = new Logger(LogCategory.TextureLoad);

    private AnimatedTexture() { }

    ~AnimatedTexture() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        _preloadFromCacheToken?.Dispose();
        _preloadFromCacheTask?.Wait();
        foreach (var texture in Textures)
            texture?.Dispose();

        Textures.Clear();
        _loaded = false;
    }

    /// <summary>
    /// Gets a reference to the first texture to display.
    /// </summary>
    /// <param name="filePath">The path to the first texture.</param>
    public void GetFirstTexturePath(out string filePath)
    {
        var file = Files[^1];
        filePath = Folder + file.RelativePath;
    }

    /// <summary>
    /// Gets a reference to the first texture to display.
    /// </summary>
    /// <param name="filePath">The path to the first texture.</param>
    public TextureRef GetFirstTexture(out string filePath)
    {
        var file = Files[^1];
        filePath = Folder + file.RelativePath;
        return TextureRef.FromFile(filePath, file.Format);
    }

    /// <summary>
    /// Pre-loads all textures into memory.
    /// </summary>
    /// <param name="firstTexReference">Native pointer to the first texture instance.</param>
    public unsafe void Preload(void* firstTexReference)
    {
        if (_loaded)
            return;
        
        var device = IoC.Get<Device>();
        Textures.Add(new SharpDX.Direct3D9.Texture((IntPtr)firstTexReference));

        if (CanLoadFromCache())
        {
            _preloadFromCacheToken = new CancellationTokenSource();
            _preloadFromCacheTask = Task.Run(() =>
            {
                PreloadFromTextureCache(device, _preloadFromCacheToken.Token);
                _loaded = true;
            });
        }
        else
        {
            PreLoadFromFilesAndCache(device, ShouldCache());
            _loaded = true;
        }
    }

    /// <summary>
    /// Gets a texture for a given frame.
    /// </summary>
    /// <param name="currentFrame">The current rendering frame.</param>
    public unsafe void* GetTextureForFrame(int currentFrame)
    {
        // Fallback for when texture still loading.
        if (!_loaded)
            return (void*)Textures[0].NativePointer;

        currentFrame %= _modulo;
        int currentIndex = _lastIndex;
        int maxIndex = _minId.Count - 1;

        // Optimization if last index was last item
        // Check between 0 and first index
        if (currentIndex == maxIndex)
        {
            // Check if still on last.
            if (currentFrame < _minId[0])
                return (void*)Textures[_lastIndex].NativePointer;

            // Else start from first element.
            currentIndex = 0;
        }

        // Loop circular once over until found.
        // Typically loop takes 1 iteration.
        do
        {
            var current = _minId[currentIndex];
            var next    = _minId[currentIndex + 1];

            if (currentFrame >= current && currentFrame < next)
            {
                _lastIndex = currentIndex;
                return (void*)Textures[currentIndex].NativePointer;
            }

            if (currentFrame == next)
            {
                _lastIndex = currentIndex + 1;
                return (void*)Textures[currentIndex + 1].NativePointer;
            }

            currentIndex++;
            currentIndex %= maxIndex;
        }
        while (currentIndex != _lastIndex);

        return (void*)Textures[_lastIndex].NativePointer;
    }

    /// <summary>
    /// Tries to create a texture
    /// Creation fails if there is not a single texture in the folder.
    /// </summary>
    /// <param name="folderPath">Path to the texture folder.</param>
    /// <param name="texture">The texture.</param>
    public static bool TryCreate(string folderPath, out AnimatedTexture texture)
    {
        try
        {
            texture = new AnimatedTexture();
            texture.Folder = Path.GetFullPath(folderPath);
            return texture.TryLoad();
        }
        catch (Exception e)
        {
            _log.WriteLine($"[{nameof(AnimatedTexture)}] Failed to make AnimatedTexture: Folder | {folderPath} | {e.Message}");
            texture = null;
            return false;
        }
    }

    /// <summary>
    /// Tried to reload this animated texture's contents.
    /// </summary>
    public bool TryReload()
    {
        Dispose();
        return TryLoad();
    }

    private bool TryLoad()
    {
        DirectorySearcher.TryGetDirectoryContents(Folder, out var files, out var directories);

        Files     = new List<AnimatedTextureFile>(files.Count);
        _minId    = new List<int>(files.Count);
        Textures  = new List<SharpDX.Direct3D9.Texture>();

        foreach (var file in files)
        {
            var format = file.FullPath.GetTextureFormatFromFileName();
            if (format == TextureFormat.Unknown)
                continue;

            var fullPath = Path.GetFullPath(file.FullPath);
            var relativePath = fullPath.Substring(Folder.Length);
            Files.Add(new AnimatedTextureFile(relativePath, format));

            if (file.LastWriteTime > _newestFileTime)
                _newestFileTime = file.LastWriteTime;
        }

        // Sort
        Files.Sort((first, second) => string.CompareOrdinal(first.RelativePath, second.RelativePath));

        foreach (var file in Files)
        {
            var path = Path.GetFileName(file.RelativePath.Substring(0, file.RelativePath.IndexOf('.')));
            int minId = Int32.Parse(path);
            _minId.Add(minId);
        }

        _modulo = _minId[^1] + 1;
        _lastIndex = 0;
        return Files.Count > 0;
    }


    private void PreLoadFromFilesAndCache(Device device, bool shouldCacheArchive)
    {
        PreloadFromFiles(device, shouldCacheArchive);
        if (shouldCacheArchive)
            CreateTextureCacheFile();
    }

    private void PreloadFromFiles(Device device, bool cacheArchiveInsteadOfFile)
    {
        // Load all files.
        for (var x = 0; x < Files.Count - 1; x++)
        {
            var file = Files[x];
            var fullPath = Folder + file.RelativePath;
            var texRef  = cacheArchiveInsteadOfFile ? TextureRef.FromFileUncached(fullPath, file.Format) : TextureRef.FromFile(fullPath, file.Format);
            var texture = SharpDX.Direct3D9.Texture.FromMemory(device, texRef.Data, Usage.Dynamic, Pool.Default);
            if (!cacheArchiveInsteadOfFile && texRef.ShouldBeCached())
            {
                var cache = TextureCacheService.Instance;
                cache?.QueueStore(fullPath, texture);
            }

            Textures.Add(texture);
        }
    }

    private unsafe void PreloadFromTextureCache(Device device, CancellationToken _asyncLoadToken)
    {
        var data = TextureCompression.PickleFromFile(CachePath);
        using var reader = new AnimatedTextureCacheReader(data);

        if (reader.FileCount != Files.Count - 1)
        {
            _log.WriteLine($"[{nameof(AnimatedTexture)}] File Count In Cache {reader.FileCount} Does Not Match Actual Count {Files.Count - 1}. Loading using fallback.");
            PreLoadFromFilesAndCache(device, ShouldCache());
            return;
        }

        while (!_asyncLoadToken.IsCancellationRequested && reader.TryGetNextFile(out int size, out byte* dataPtr))
        {
            var unmangedStream = new DataStream((IntPtr)dataPtr, size, true, true);
            var texture = SharpDX.Direct3D9.Texture.FromStream(device, unmangedStream, Usage.Dynamic, Pool.Default);
            Textures.Add(texture);
        }
    }

    private void CreateTextureCacheFile()
    {
        // We dump the data using DirectX here because we want the DDS format; as that contains mipmaps.
        // If the source file was not a DDS file (in which case you should yell at the mod creator!)
        // then we will at least have mipmaps.
        if (Textures.Count <= 1)
            return;

        var cacheFiles = new List<byte[]>(Textures.Count - 1);
        for (int x = 1; x < Textures.Count; x++)
        {
            var texture = Textures[x];
            using var stream = BaseTexture.ToStream(texture, ImageFileFormat.Dds);
            cacheFiles.Add(stream.ReadRange<byte>((int)stream.RemainingLength));
        }

        // Silently Cache and Compress
        Task.Run(() =>
        {
            using var cacheWriter = new AnimatedTextureCacheWriter((cacheFiles[0].Length * cacheFiles.Count) + 16, cacheFiles.Count);
            foreach (var file in cacheFiles)
                cacheWriter.TryWriteFile(file);

            cacheWriter.Finish();
            Directory.CreateDirectory(Path.GetDirectoryName(CachePath));
            TextureCompression.PickleToFile(CachePath, cacheWriter.GetSpan(), LZ4Level.L11_OPT);
        });
    }

    /// <summary>
    /// Returns true if a list of textures should be cached; else false.
    /// </summary>
    private bool ShouldCache() => Files.Count >= MinFilesBeforeArchiveCache;

    private bool CanLoadFromCache()
    {
        try
        {
            return File.GetLastWriteTimeUtc(CachePath) > _newestFileTime;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
