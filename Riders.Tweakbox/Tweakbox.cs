using System;
using System.Collections.Generic;
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
using Reloaded.Mod.Interfaces;
using Reloaded.Universal.Redirector.Interfaces;
using Riders.Tweakbox.Components.Debug;
using Riders.Tweakbox.Components.Debug.Log;
using Riders.Tweakbox.Components.Editors.Gear;
using Riders.Tweakbox.Components.Editors.Layout;
using Riders.Tweakbox.Components.Editors.Physics;
using Riders.Tweakbox.Components.Netplay;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.Functions;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace Riders.Tweakbox
{
    public class Tweakbox
    {
        /* Class Declarations */
        public ImguiHook Hook { get; private set; }
        public IReloadedHooks Hooks { get; private set; }
        public IReloadedHooksUtilities HooksUtilities { get; private set; }
        public IRedirectorController Redirector { get; private set; }

        public bool InputsEnabled { get; private set; } = true;
        public bool IsEnabled { get; private set; } = true;
        public bool IsReady { get; private set; } = false;
        public MenuBar MenuBar { get; private set; }
        public List<IController> Controllers { get; private set; } = new List<IController>();
        public IHook<Functions.CdeclReturnIntFn> BlockInputsHook { get; private set; }
        public event Action OnInitialized;

        private WelcomeScreenRenderer _welcomeScreenRenderer;
        private DllNotifier _notifier;
        private IModLoader _modLoader;

        /* Creation & Disposal */
        private Tweakbox(){}

        /// <summary>
        /// Creates a new instance of Riders Tweakbox.
        /// </summary>
        public static async Task<Tweakbox> Create(IReloadedHooks hooks, IReloadedHooksUtilities hooksUtilities, IRedirectorController redirector, IModLoader modLoader)
        {
            var modFolder       = modLoader.GetDirectoryForModId("Riders.Tweakbox");
            var tweakBox        = new Tweakbox();

            tweakBox._notifier  = new DllNotifier(hooks);
            tweakBox._modLoader = modLoader;
            tweakBox.Hooks = hooks;
            tweakBox.HooksUtilities = hooksUtilities;
            tweakBox.Redirector = redirector;
            tweakBox.BlockInputsHook = Functions.GetInputs.Hook(tweakBox.BlockGameInputsIfEnabled).Activate();
            tweakBox.InitializeIoC(modFolder);
            tweakBox.MenuBar = new MenuBar()
            {
                Menus = new List<MenuBarItem>()
                {
                    new MenuBarItem("Netplay", new List<IComponent>()
                    {
                        IoC.GetSingleton<NetplayMenu>()
                    }),
                    new MenuBarItem("Tweaks", new List<IComponent>()
                    {
                        IoC.GetSingleton<TweakboxSettings>(),
                        IoC.GetSingleton<TextureEditor>(),
                    }),
                    new MenuBarItem("Editors", new List<IComponent>()
                    {
                        IoC.GetSingleton<GearEditor>(),
                        IoC.GetSingleton<PhysicsEditor>(),
                        IoC.GetSingleton<LayoutEditor>(),
                    }),
                    new MenuBarItem("Debug", new List<IComponent>()
                    {
                        IoC.GetSingleton<DemoWindow>(),
                        IoC.GetSingleton<UserGuideWindow>(),
                        IoC.GetSingleton<ShellTestWindow>(),
                        IoC.GetSingleton<TaskTrackerWindow>(),
                        IoC.GetSingleton<MemoryDebugWindow>(),
                        IoC.GetSingleton<LogWindow>(),
                        IoC.GetSingleton<RaceSettingsWindow>(),
                        IoC.GetSingleton<DolphinDumperWindow>(),
                        IoC.GetSingleton<LapCounterWindow>(),
                        IoC.GetSingleton<ServerBrowserDebugWindow>(),
                    })
                },
                Text = new List<string>()
                {
                    "F11: Show/Hide Menus",
                    "F10: Enable/Disable Game Input"
                }
            };

            tweakBox.Hook = await ImguiHook.Create(tweakBox.Render);

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
        private void InitializeIoC(string modFolder)
        {
            var io = new IO(modFolder);
            IoC.Kernel.Bind<IO>().ToConstant(io);
            IoC.Kernel.Bind<Tweakbox>().ToConstant(this);
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
                IoC.GetSingleton(type);

            // Initialize all services.
            var serviceTypes = types.Where(x => typeof(ISingletonService).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
            foreach (var type in serviceTypes)
                IoC.GetSingleton(type);

            // Initialize all controllers.
            var controllerTypes = types.Where(x => typeof(IController).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
            foreach (var type in controllerTypes)
                Controllers.Add(IoC.GetSingleton<IController>(type));
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
        public void EnableCrashDumps()
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

        private int BlockGameInputsIfEnabled()
        {
            // Skips game controller input obtain function is menu is open.
            if (InputsEnabled)
                return BlockInputsHook.OriginalFunction();

            return 0;
        }

        /* Implementation */
        private void Render()
        { 
            if (!IsReady)
                return;

            // This works because the keys sent to imgui in WndProc follow
            // the Windows key code order.
            if (ImGui.IsKeyPressed((int) Keys.F11, false))
                IsEnabled = !IsEnabled;

            if (ImGui.IsKeyPressed((int)Keys.F10, false))
                InputsEnabled = !InputsEnabled;

            // Update Menu Bar Text
            if (InputsEnabled)
                MenuBar.Text[1] = "F10: Disable Game Input";
            else
                MenuBar.Text[1] = "F10: Enable Game Input";

            // Render MenuBar and Menus
            if (IsEnabled) 
                MenuBar.Render();

            // Render Shell
            Shell.Render();
        }
    }
}
