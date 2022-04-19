using Riders.Tweakbox.Services.Common;

namespace Riders.Tweakbox.Services.ObjectLayout;

/// <summary>
/// Watches over a specified folder and subfolders for object layout files in real time.
/// Creates a map of file name to list of files.
/// </summary>
public class ObjectLayoutDictionary : FileDictionary
{
    public const string LayoutExtension = ".layout";

    public ObjectLayoutDictionary(string source, string extension = LayoutExtension) : base(source, extension) { }

    public ObjectLayoutDictionary() { }

    public override void Initialize(string source) => InitializeBase(source, LayoutExtension);
}
