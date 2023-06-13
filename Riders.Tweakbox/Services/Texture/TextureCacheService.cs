using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MessagePack;
using MessagePack.Resolvers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Services.Interfaces;
using Riders.Tweakbox.Services.Texture.Enums;
using Riders.Tweakbox.Services.Texture.Structs;
using SharpDX;
using SharpDX.Direct3D9;
namespace Riders.Tweakbox.Services.Texture;

/// <summary>
/// A service that allows for storing of DDS texture data corresponding to a given non-DDS source file.
/// </summary>
public class TextureCacheService : ISingletonService
{
    public static TextureCacheService Instance { get; private set; }

    private const int CacheExpiryDays = 64;
    private const int SaveCacheFilePeriodMs = 5000;

    private Dictionary<string, TextureCacheEntry> _textureCache = new Dictionary<string, TextureCacheEntry>(StringComparer.OrdinalIgnoreCase);
    private bool _isInvalidated;
    private IO _io;
    private Timer _timer;
    private Logger _log = new Logger(LogCategory.TextureLoad);

    public TextureCacheService(IO io)
    {
        _io = io;
        Load();

        _timer = new Timer(SaveIfInvalidated, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(SaveCacheFilePeriodMs));
        Instance = this;
    }

    /// <summary>
    /// Adds texture data to the cache.
    /// </summary>
    /// <param name="filePath">The path of the file to cache.</param>
    /// <param name="data">The raw DDS texture data to cache.</param>
    public void Store(string filePath, Span<byte> data)
    {
        if (!_textureCache.TryGetValue(filePath, out var entry))
            entry = new TextureCacheEntry();
        
        entry.LastWriteTime = File.GetLastWriteTimeUtc(filePath);
        entry.LastAccessTime = DateTime.UtcNow;
        if (string.IsNullOrEmpty(entry.Target))
            entry.Target = Path.GetFileName(GetRandomUniqueFilePath());

        TextureCompression.PickleToFile(entry.GetFullFilePath(_io), data);
        _textureCache[filePath] = entry;
        _isInvalidated = true;
    }

    /// <summary>
    /// Adds texture data to the cache.
    /// </summary>
    /// <param name="filePath">The path of the file to cache.</param>
    /// <param name="stream">The stream containing DDS texture data.</param>
    public unsafe void Store(string filePath, DataStream stream) => Store(filePath, new Span<byte>((void*)stream.DataPointer, (int)stream.Length));

    /// <summary>
    /// Adds texture data to the cache.
    /// </summary>
    /// <param name="filePath">The path of the file to cache.</param>
    /// <param name="texture">The texture to store.</param>
    public unsafe void Store(string filePath, SharpDX.Direct3D9.Texture texture) => Store(filePath, BaseTexture.ToStream(texture, ImageFileFormat.Dds));

    /// <summary>
    /// Queues a store operation to the cache to the default <see cref="ThreadPool"/>.
    /// </summary>
    /// <param name="filePath">The path of the file to cache.</param>
    /// <param name="texture">The texture to store.</param>
    public void QueueStore(string filePath, SharpDX.Direct3D9.Texture texture)
    {
        var stream = BaseTexture.ToStream(texture, ImageFileFormat.Dds);
        ThreadPool.QueueUserWorkItem(state => Store(filePath, stream));
    }

    /// <summary>
    /// Attempts to grab texture data from the cache.
    /// </summary>
    /// <param name="filePath">The name of the file to fetch from cache.</param>
    /// <param name="result">The texture, ready for loading.</param>
    public bool TryFetch(string filePath, out TextureRef result)
    {
        result = default;
        if (!_textureCache.TryGetValue(filePath, out var entry))
            return false;

        try
        {
            var lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            if (lastWriteTime != entry.LastWriteTime)
            {
                _log.WriteLine("Texture write time mismatch. Removing from cache.");
                InvalidateKey(filePath);
                return false;
            }

            entry.LastAccessTime = DateTime.UtcNow;
            var data = TextureCompression.PickleFromFile(entry.GetFullFilePath(_io));
            result = new TextureRef(data, TextureFormat.Dds, true);
            _textureCache[filePath] = entry;
            _isInvalidated = true;
            return true;
        }
        catch (Exception e)
        {
            _log.WriteLine("Failed to fetch texture from cache. " + e.Message);
            return false;
        }
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_io.TextureCacheFilePath))
            {
                var data = File.ReadAllBytes(_io.TextureCacheFilePath);
                _textureCache = MessagePackSerializer.Deserialize<Dictionary<string, TextureCacheEntry>>(data, ContractlessStandardResolver.Options);
                InvalidateOldItems();
            }
        }
        catch (Exception e)
        {
            _log.WriteLine("Failed to load Texture Cache File. " + e.Message);
        }

        DeleteLooseFiles();
    }

    private void Save()
    {
        try
        {
            var data = MessagePackSerializer.Serialize<Dictionary<string, TextureCacheEntry>>(_textureCache, ContractlessStandardResolver.Options);
            File.WriteAllBytes(_io.TextureCacheFilePath, data);
            _log.WriteLine("Saved Texture Cache.");
        }
        catch (Exception e)
        {
            _log.WriteLine("Failed to save Texture Cache File. " + e.Message);
        }
    }

    private void InvalidateOldItems()
    {
        var toRemove = new List<string>();
        var now = DateTime.UtcNow;
        var maxDuration = TimeSpan.FromDays(CacheExpiryDays);

        foreach (var cacheItem in _textureCache)
        {
            var entry = cacheItem.Value;
            var timeSince = now - entry.LastAccessTime;
            if (timeSince <= maxDuration && File.Exists(entry.GetFullFilePath(_io)) && File.Exists(cacheItem.Key))
                continue;

            toRemove.Add(cacheItem.Key);
        }

        foreach (var item in toRemove)
            InvalidateKey(item);
    }

    private void DeleteLooseFiles()
    {
        var targetSet = new HashSet<string>();
        foreach (var cache in _textureCache)
            targetSet.Add(cache.Value.Target);

        var files = Directory.GetFiles(_io.TextureCacheFilesFolder).Select(Path.GetFileName);
        foreach (var file in files)
        {
            if (targetSet.Contains(file))
                continue;

            try
            {
                File.Delete(Path.Combine(_io.TextureCacheFilesFolder, file));
            }
            catch (Exception e)
            {
                _log.WriteLine("Texture Cache: Failed to delete loose file. " + e.Message);
            }
        }
    }

    private void InvalidateKey(string filePath)
    {
        if (_textureCache.TryGetValue(filePath, out var entry))
        {
            try
            {
                File.Delete(entry.Target);
            }
            catch (Exception e)
            {
                _log.WriteLine("Texture Cache: Failed to delete old item. " + e.Message);
            }
        }

        _textureCache.Remove(filePath);
        _isInvalidated = true;
    }

    private string GetRandomUniqueFilePath()
    {
        string filePath;

        do
        {
            filePath = Path.Combine(_io.TextureCacheFilesFolder, Path.GetRandomFileName()) + ".dds.lz4";
        }
        while (File.Exists(filePath));

        return filePath;
    }

    private void SaveIfInvalidated(object? state)
    {
        if (_isInvalidated)
        {
            Save();
            _isInvalidated = false;
        }
    }
}
