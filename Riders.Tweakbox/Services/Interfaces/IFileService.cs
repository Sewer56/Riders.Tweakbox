using System.Collections.Generic;
using Reloaded.Mod.Interfaces;

namespace Riders.Tweakbox.Services.Interfaces;

public interface IFileService
{
    /// <summary>
    /// List of all active file services.
    /// </summary>
    public static List<IFileService> Services { get; private set; } = new List<IFileService>();

    /// <summary>
    /// Seeds all available services.
    /// </summary>
    public static void SeedAll(int seed)
    {
        foreach (var service in Services)
            service.SeedRandom(seed);
    }

    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    IModLoader ModLoader { get; }

    /// <summary>
    /// Path to the folder (relative to mod folder) storing files to be monitored.
    /// </summary>
    string RedirectFolderPath { get; }

    /// <summary>
    /// The seed with which the random component was last initialised.
    /// </summary>
    int Seed { get; }

    /// <summary>
    /// Seeds the random number generator tied to this service.
    /// </summary>
    void SeedRandom(int seed);

    /// <summary>
    /// Obtains all potential candidate files for a given file name.
    /// </summary>
    /// <param name="fileName">The name of the file the alternatives for.</param>
    /// <param name="files">List of files to add the candidates to.</param>
    void GetFilesForFileName(string fileName, List<string> files);
}