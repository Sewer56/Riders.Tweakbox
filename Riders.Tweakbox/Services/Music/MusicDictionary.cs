using Riders.Tweakbox.Services.Common;

namespace Riders.Tweakbox.Services.Music;

/// <summary>
/// Watches over a specified folder and subfolders for ADX files in real time.
/// Creates a map of file name to list of files.
/// </summary>
public class MusicDictionary : FileDictionary
{
    public MusicDictionary(string source, string extension = MusicCommon.AdxExtension) : base(source, extension) { }
}
