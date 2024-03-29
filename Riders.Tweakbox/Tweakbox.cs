﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DearImguiSharp;
using Microsoft.Win32;
using Reloaded.Assembler;
using Reloaded.Hooks.Definitions;
using Reloaded.Imgui.Hook;
using Reloaded.Imgui.Hook.Implementations;
using Reloaded.Mod.Interfaces;
using Reloaded.Universal.Redirector.Interfaces;
using Riders.Controller.Hook.Interfaces;
using Riders.Tweakbox.Components.Main;
using Riders.Tweakbox.Components.Debug;
using Riders.Tweakbox.Components.Debug.Log;
using Riders.Tweakbox.Components.Editors.Gear;
using Riders.Tweakbox.Components.Editors.Info;
using Riders.Tweakbox.Components.Editors.Layout;
using Riders.Tweakbox.Components.Editors.Physics;
using Riders.Tweakbox.Components.Netplay;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Services.Interfaces;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Parser.Archive;
using static Riders.Tweakbox.Misc.BenchmarkUtilities;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;
using Riders.Tweakbox.Misc.Data;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Writers;
using Riders.Tweakbox.Api;
using Riders.Tweakbox.Components.Debug.NN;
using Riders.Tweakbox.Components.Editors.Menu;
using Riders.Tweakbox.Components.Editors.Rail;
using Riders.Tweakbox.Interfaces;

namespace Riders.Tweakbox;

public class Tweakbox
{
    /* Class Declarations */
    public IReloadedHooks Hooks { get; private set; }
    public IReloadedHooksUtilities HooksUtilities { get; private set; }
    public IRedirectorController Redirector { get; private set; }
    
    public bool IsEnabled { get; private set; } = true;
    public bool IsReady { get; private set; } = false;
    public MenuBar MenuBar { get; private set; }
    public List<IController> Controllers { get; private set; } = new List<IController>();
    public event Action OnFrame;

    public event Action OnInitialized;

    private WelcomeScreenRenderer _welcomeScreenRenderer;
    private DllNotifier _notifier;
    private IModLoader _modLoader;
    private IControllerHook _controllerHook;
    private Logger _log = new Logger(LogCategory.Default);

    /* Creation & Disposal */
    private Tweakbox() { }

    /// <summary>
    /// Creates a new instance of Riders Tweakbox.
    /// </summary>
    public static async Task<Tweakbox> Create(IReloadedHooks hooks, IReloadedHooksUtilities hooksUtilities, IRedirectorController redirector, IModLoader modLoader, IControllerHook controllerHook, Program program)
    {
        var modFolder    = modLoader.GetDirectoryForModId(program.ModId);
        var configFolder = modLoader.GetModConfigDirectory(program.ModId);
        var tweakBox = new Tweakbox();

        tweakBox._notifier = new DllNotifier(hooks);
        tweakBox._modLoader = modLoader;
        tweakBox._controllerHook = controllerHook;
        tweakBox.Hooks = hooks;
        tweakBox.HooksUtilities = hooksUtilities;
        tweakBox.Redirector = redirector;
        tweakBox.TryDecompressFiles();
        tweakBox.InitializeIoC(modFolder, configFolder);

        tweakBox.MenuBar = new MenuBar()
        {
            Menus = new List<MenuBarItem>()
            {
                new MenuBarItem("Main", new List<IComponent>()
                {
                    Benchmark(() => IoC.GetSingleton<NetplayMenu>(), nameof(NetplayMenu)),
                    Benchmark(() => IoC.GetSingleton<UserGuideWindow>(), nameof(UserGuideWindow)),
                    Benchmark(() => IoC.GetSingleton<AboutMenu>(), nameof(AboutMenu)),
                    Benchmark(() => IoC.GetSingleton<OpenSourceLibraries>(), nameof(OpenSourceLibraries)),
                }),
                new MenuBarItem("Settings", new List<IComponent>()
                { 
                    Benchmark(() => IoC.GetSingleton<TweakboxSettings>(), nameof(TweakboxSettings)),
                    Benchmark(() => IoC.GetSingleton<TextureEditor>(), nameof(TextureEditor)),
                    Benchmark(() => IoC.GetSingleton<InfoEditor>(), nameof(InfoEditor)),
                    Benchmark(() => IoC.GetSingleton<CustomGearSettings>(), nameof(CustomGearSettings)),
                }),
                new MenuBarItem("Editors", new List<IComponent>()
                {
                    Benchmark(() => IoC.GetSingleton<GearEditor>(), nameof(GearEditor)),
                    Benchmark(() => IoC.GetSingleton<PhysicsEditor>(), nameof(PhysicsEditor)),
                    Benchmark(() => IoC.GetSingleton<LayoutEditor>(), nameof(LayoutEditor)),
                    Benchmark(() => IoC.GetSingleton<RailSpeedEditor>(), nameof(RailSpeedEditor)),
                }),
                new MenuBarItem("Debug", new List<IComponent>()
                {
                    Benchmark(() => IoC.GetSingleton<DemoWindow>(), nameof(DemoWindow)),
                    Benchmark(() => IoC.GetSingleton<ShellTestWindow>(), nameof(ShellTestWindow)),
                    Benchmark(() => IoC.GetSingleton<TaskTrackerWindow>(), nameof(TaskTrackerWindow)),
                    Benchmark(() => IoC.GetSingleton<MemoryDebugWindow>(), nameof(MemoryDebugWindow)),
                    Benchmark(() => IoC.GetSingleton<LogWindow>(), nameof(LogWindow)),
                    Benchmark(() => IoC.GetSingleton<RaceSettingsWindow>(), nameof(RaceSettingsWindow)),
                    Benchmark(() => IoC.GetSingleton<DolphinDumperWindow>(), nameof(DolphinDumperWindow)),
                    Benchmark(() => IoC.GetSingleton<LapCounterWindow>(), nameof(LapCounterWindow)),
                    Benchmark(() => IoC.GetSingleton<HeapViewerWindow>(), nameof(HeapViewerWindow)),
                    Benchmark(() => IoC.GetSingleton<ChatMenuDebug>(), nameof(ChatMenuDebug)),
                    Benchmark(() => IoC.GetSingleton<SlipstreamDebug>(), nameof(SlipstreamDebug)),
                    Benchmark(() => IoC.GetSingleton<MenuEditor>(), nameof(MenuEditor)),

                    Benchmark(() => IoC.GetSingleton<BonePreviewer>(), nameof(BonePreviewer)),
#if DEBUG
                    Benchmark(() => IoC.GetSingleton<ServerBrowserDebugWindow>(), nameof(ServerBrowserDebugWindow)),
#endif
                })
            },
            Text = new List<string>()
            {
                "F11: Show/Hide Menus"
            }
        };

        // Register API
        var api = IoC.GetSingleton<TweakboxApi>();
        tweakBox._modLoader.AddOrReplaceController<ITweakboxApi>(program, api);

        // Wait for Overlay.
        await ImguiHook.Create(tweakBox.Render, new ImguiHookOptions()
        {
            EnableViewports = false,
            Implementations = new List<IImguiHook>()
            {
                new ImguiHookDx9()
            }
        });

        // Post-setup steps
        Shell.SetupImGuiConfig(modFolder);
        tweakBox.EnableCrashDumps();
        tweakBox.DisplayFirstTimeDialog();
        
        tweakBox.IsReady = true;
        tweakBox.OnInitialized?.Invoke();
        return tweakBox;
    }

    /// <summary>
    /// Initializes global bindings.
    /// </summary>
    private void InitializeIoC(string modFolder, string configFolder)
    {
        var io = new IO(modFolder, configFolder);
        IoC.Kernel.Bind<IO>().ToConstant(io);
        IoC.Kernel.Bind<Tweakbox>().ToConstant(this);
        IoC.Kernel.Bind<IControllerHook>().ToConstant(_controllerHook);
        IoC.Kernel.Bind<IModLoader>().ToConstant(_modLoader);
        IoC.Kernel.Bind<IReloadedHooks>().ToConstant(Hooks);
        IoC.Kernel.Bind<Reloaded.Hooks.Definitions.IReloadedHooks>().ToConstant(Hooks);
        IoC.Kernel.Bind<IRedirectorController>().ToConstant(Redirector);
        IoC.Kernel.Bind<IReloadedHooksUtilities>().ToConstant(HooksUtilities);
        IoC.Kernel.Bind<Assembler>().ToConstant(new Assembler());

        var types = Assembly.GetExecutingAssembly().GetTypes();

        // Initialize all configs.
        var configTypes = types.Where(x => typeof(IConfiguration).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        foreach (var type in configTypes)
            Benchmark(() => IoC.GetSingleton(type), type.FullName);

        // Initialize all services.
        var serviceTypes = types.Where(x => typeof(ISingletonService).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        foreach (var type in serviceTypes)
            Benchmark(() => IoC.GetSingleton(type), type.FullName);

        // Initialize all controllers.
        var controllerTypes = types.Where(x => typeof(IController).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        foreach (var type in controllerTypes)
            Benchmark(() => Controllers.Add(IoC.GetSingleton<IController>(type)), type.FullName);
    }

    private void DisplayFirstTimeDialog()
    {
        var io = IoC.Get<IO>();
        _welcomeScreenRenderer = new WelcomeScreenRenderer();

        // First time message
        if (!File.Exists(io.FirstTimeFlagPath))
        {
            File.Create(io.FirstTimeFlagPath);
            Shell.AddCustom(_welcomeScreenRenderer.RenderFirstTimeDialog);
        }
    }

    /// <summary>
    /// Enables crash dumps for Sonic Riders.
    /// </summary>
    private void EnableCrashDumps()
    {
        const string dumpsConfigRegkeyPath = @"SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps";

        try
        {
            var localMachineKey = Environment.Is64BitOperatingSystem ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var key = localMachineKey.OpenSubKey(dumpsConfigRegkeyPath, false);
            if (key != null)
                return;

            if (localMachineKey.CreateSubKey(dumpsConfigRegkeyPath) == null)
                ShowFailureDialog();
        }
        catch (Exception e)
        {
            ShowFailureDialog();
        }

        void ShowFailureDialog() => Shell.AddDialog("About Crash Dumps", "Tweakbox couldn't enable crash dumps necessary for reporting Netplay Crashes.\n" +
                                                                         "Please run from Reloaded as admin at least once, thanks!");
    }

    private void TryDecompressFiles()
    {
        try
        {
            Redirector.Disable();

            // Check using one file, if it is compressed; decompress all.
            using var checkStream = new FileStream(Path.Combine(IO.DataFolderLocation, "PS00"), FileMode.Open, FileAccess.Read);
            var isSonicCompressed = ArchiveCompression.IsCompressed(checkStream, false);
            if (!isSonicCompressed)
                return;

            checkStream.Dispose();
            _log.WriteLine($"[{nameof(Tweakbox)}] Decompressing Game Files... This might take a minute; hold tight!");
            _log.WriteLine($"[{nameof(Tweakbox)}] This is a one time operation; intended to prevent loading screen freezes in vanilla game.");
            DirectorySearcher.GetDirectoryContentsRecursive(IO.DataFolderLocation, out var files, out var directories);

            Span<byte> test = stackalloc byte[8];
            for (var x = 0; x < files.Count; x++)
            {
                // Skip if has extension.
                var file = files[x];
                if (Path.GetExtension(file.FullPath) != "")
                    continue;

                try
                {
                    using var fileStream = new FileStream(file.FullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    if (!ArchiveCompression.IsCompressed(fileStream, false))
                        continue;

                    // Decompress and write.
                    var uncompressed = ArchiveCompression.DecompressFast(fileStream, (int)fileStream.Length, ArchiveCompressorOptions.PC);
                    fileStream.SetLength(uncompressed.Length);
                    fileStream.Position = 0;
                    fileStream.Write(uncompressed);

                    // Report Back
                    if (x % 75 == 0)
                        _log.WriteLine($"Files Processed: {x} / {files.Count}");
                }
                catch (Exception ex)
                {
                    _log.WriteLine($"Failed to decompress file: {ex.Message}");
                }
            }
        }
        finally
        {
            Redirector.Enable();
        }
    }

    /* Implementation */
    private void Render()
    {
        if (!IsReady)
            return;

        try
        {
            // This works because the keys sent to imgui in WndProc follow
            // the Windows key code order.
            if (ImGui.IsKeyPressed((int)VK.F11, false))
                IsEnabled = !IsEnabled;

            // Render MenuBar and Menus
            if (IsEnabled)
                MenuBar.Render();

            // Render Shell
            Shell.Render();
            OnFrame?.Invoke();
        }
        catch (Exception e)
        {
            _log.WriteLine($"Unhandled Exception in Tweakbox Code:\n{e.Message}\n{e.StackTrace}");
        }
    }
}
