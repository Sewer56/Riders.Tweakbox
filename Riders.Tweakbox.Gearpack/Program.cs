using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Riders.Tweakbox.Interfaces;
using System;

namespace Riders.Tweakbox.Gearpack;

public class Program : IMod
{
    /// <summary>
    /// Your mod if from ModConfig.json, used during initialization.
    /// </summary>
    private const string MyModId = "Riders.Tweakbox.Gearpack";

    /// <summary>
    /// Used for writing text to the console window.
    /// </summary>
    private ILogger _logger;

    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private IModLoader _modLoader;
    private CustomGearPack _customGearPack;
    private ITweakboxApi _api;
    private ITweakboxApiImpl _apiImpl;

    private string _apiModName;

    public void StartEx(IModLoaderV1 loader, IModConfigV1 config)
    {
        _modLoader = (IModLoader)loader;
        _logger = (ILogger)_modLoader.GetLogger();

        /* Your mod code starts here. */
        _modLoader.GetController<ITweakboxApi>().TryGetTarget(out _api);
        _apiModName = $"{config.ModId}.{config.ModVersion}";
        _apiImpl = _api.Register(_apiModName);
        _customGearPack = new CustomGearPack(_modLoader.GetDirectoryForModId(MyModId), _apiImpl);
    }

    /* Mod loader actions. */
    public void Suspend() { Unload(); }

    public void Resume()
    {
        _apiImpl = _api.Register(_apiModName);
        _customGearPack.Unload(_apiImpl);
    }

    public void Unload()
    {
        _customGearPack.Unload(_apiImpl);
        _api.Unregister(_apiModName);
    }

    /*  If CanSuspend == false, suspend and resume button are disabled in Launcher and Suspend()/Resume() will never be called.
        If CanUnload == false, unload button is disabled in Launcher and Unload() will never be called.
    */
    public bool CanUnload() => false;
    public bool CanSuspend() => true;

    /* Automatically called by the mod loader when the mod is about to be unloaded. */
    public Action Disposing { get; }

    /* This is a dummy for R2R (ReadyToRun) deployment.
       For more details see: https://github.com/Reloaded-Project/Reloaded-II/blob/master/Docs/ReadyToRun.md
    */
    public static void Main() { }

}
