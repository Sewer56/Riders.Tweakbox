using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;
using System.IO;

namespace Riders.Tweakbox.Services.Placeholder;

/// <summary>
/// Service which provides placeholder textures for various things.
/// </summary>
public class PlaceholderTextureService : ISingletonService
{
    // Folder path for placeholder dummy images.
    private const string _bikeRelativePath = "CSS/Placeholder/Bike";
    private const string _skateRelativePath = "CSS/Placeholder/Skate";
    private const string _boardRelativePath = "CSS/Placeholder/Board";
    private const string _titleRelativePath = "CSS/Placeholder/Title";

    // Placeholder icon paths.
    public string[] SkateIconPlaceholders;
    public string[] BikeIconPlaceholders;
    public string[] BoardIconPlaceholders;
    public string[] TitleIconPlaceholders;

    public PlaceholderTextureService(IO io)
    {
        // Get dummy folder.
        var bikeTextureFolder = Path.Combine(io.AssetsFolder, _bikeRelativePath);
        var skateTextureFolder = Path.Combine(io.AssetsFolder, _skateRelativePath);
        var boardTextureFolder = Path.Combine(io.AssetsFolder, _boardRelativePath);
        var titleTextureFolder = Path.Combine(io.AssetsFolder, _titleRelativePath);

        SkateIconPlaceholders = Directory.GetFiles(skateTextureFolder);
        BikeIconPlaceholders = Directory.GetFiles(bikeTextureFolder);
        BoardIconPlaceholders = Directory.GetFiles(boardTextureFolder);
        TitleIconPlaceholders = Directory.GetFiles(titleTextureFolder);
    }
}
