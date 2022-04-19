using Riders.Tweakbox.Services.Common;

namespace Riders.Tweakbox.Services.Rails;

/// <summary>
/// Watches over a specified folder and subfolders for object layout files in real time.
/// Creates a map of file name to list of files.
/// </summary>
public class RailDictionary : FileDictionary
{
    public const string LayoutExtension = ".json";

    public RailDictionary(string source, string extension = LayoutExtension) : base(source, extension) { }

    public RailDictionary() { }

    public override void Initialize(string source) => InitializeBase(source, LayoutExtension);
}