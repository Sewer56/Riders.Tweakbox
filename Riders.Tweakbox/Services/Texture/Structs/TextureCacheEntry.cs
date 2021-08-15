using System;
using System.IO;
using MessagePack;
using Riders.Tweakbox.Misc;
namespace Riders.Tweakbox.Services.Texture.Structs;

[MessagePackObject(false)]
public struct TextureCacheEntry
{
    /// <summary>
    /// Name of the file inside the cache folder containing cached data.
    /// </summary>
    [Key(0)]
    public string Target;

    /// <summary>
    /// The last write time of the file.
    /// </summary>
    [Key(1)]
    public DateTime LastWriteTime;

    /// <summary>
    /// Last time this cache entry was accessed.
    /// </summary>
    [Key(2)]
    public DateTime LastAccessTime;

    public string GetFullFilePath(IO io) => Path.Combine(io.TextureCacheFilesFolder, Target);
}
