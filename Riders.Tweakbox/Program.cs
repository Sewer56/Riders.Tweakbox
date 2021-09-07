using System;
using System.Diagnostics;
using System.Runtime;
using System.Windows;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Universal.Redirector.Interfaces;
using Riders.Controller.Hook.Interfaces;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Sewer56.Hooks.Utilities;
using Sewer56.Imgui.Utilities;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace Riders.Tweakbox;
public class Program : IMod, IExports
{
    /// <summary>
    /// Used for writing text to the console window.
    /// </summary>
    private ILogger _logger;

    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private IModLoader _modLoader;

    private Tweakbox _tweakbox;

    /// <summary>
    /// Entry point for your mod.
    /// </summary>
    public async void Start(IModLoaderV1 loader)
    {
#if DEBUG
        MessageBox.Show("Attach Debugger Now");
#endif
        _modLoader = (IModLoader)loader;
        _logger = (ILogger)_modLoader.GetLogger();
        _modLoader.GetController<IReloadedHooks>().TryGetTarget(out var hooks);
        _modLoader.GetController<IReloadedHooksUtilities>().TryGetTarget(out var hooksUtilities);
        _modLoader.GetController<IRedirectorController>().TryGetTarget(out var redirector);
        _modLoader.GetController<IControllerHook>().TryGetTarget(out var controllerHook);

        /* Your mod code starts here. */
        Log.ConsoleListener = new ConsoleOutListener(_logger);
        Log.HudListener = new ShellTraceListener();
        Sewer56.SonicRiders.SDK.Init(hooks);
        Reloaded.Imgui.Hook.SDK.Init(hooks);
        AsmHelpers.Init(hooks);
#if DEBUG
        Reloaded.Imgui.Hook.SDK.Debug += s => Log.WriteLine(s);
#endif
        _tweakbox = await Tweakbox.Create(hooks, hooksUtilities, redirector, _modLoader, controllerHook, this);

        // Tweak Garbage Collection.
        GC.Collect();
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        // Tweak Process Priority
        var process = Process.GetCurrentProcess();
        process.PriorityClass = ProcessPriorityClass.High;
    }

    /* Mod loader actions. */
    public void Suspend() { }
    public void Resume() { }
    public void Unload() { }

    /*  If CanSuspend == false, suspend and resume button are disabled in Launcher and Suspend()/Resume() will never be called.
        If CanUnload == false, unload button is disabled in Launcher and Unload() will never be called.
    */
    public bool CanUnload() => false;
    public bool CanSuspend() => false;

    /* Automatically called by the mod loader when the mod is about to be unloaded. */
    public Action Disposing { get; }

    /* This is a dummy for R2R (ReadyToRun) deployment.
       For more details see: https://github.com/Reloaded-Project/Reloaded-II/blob/master/Docs/ReadyToRun.md
    */
    public static void Main() { }
    public Type[] GetTypes() => new[] { typeof(ITweakboxApi) };
}
