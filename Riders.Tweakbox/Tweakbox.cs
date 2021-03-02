using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using DearImguiSharp;
using Microsoft.Win32;
using Reloaded.Hooks.Definitions;
using Reloaded.Imgui.Hook;
using Riders.Tweakbox.Components.Debug;
using Riders.Tweakbox.Components.Debug.Log;
using Riders.Tweakbox.Components.Editors.Gear;
using Riders.Tweakbox.Components.Editors.Physics;
using Riders.Tweakbox.Components.Netplay;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls.Extensions;
using Sewer56.Imgui.Layout;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.Imgui.Utilities;
using Sewer56.SonicRiders.Functions;
using static DearImguiSharp.ImGuiWindowFlags;
using Constants = Sewer56.Imgui.Misc.Constants;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace Riders.Tweakbox
{
    public class Tweakbox
    {
        /* Class Declarations */
        public ImguiHook Hook { get; private set; }
        public IReloadedHooks Hooks { get; private set; }
        public IReloadedHooksUtilities HooksUtilities { get; private set; }

        public bool InputsEnabled { get; private set; } = true;
        public bool IsEnabled { get; private set; } = true;
        public bool IsReady { get; private set; } = false;
        public MenuBar MenuBar { get; private set; }
        public List<IController> Controllers { get; private set; } = new List<IController>();
        public IHook<Functions.CdeclReturnIntFn> BlockInputsHook { get; private set; }
        private WelcomeScreenRenderer _welcomeScreenRenderer;

        /* Creation & Disposal */
        private Tweakbox(){}

        /// <summary>
        /// Creates a new instance of Riders Tweakbox.
        /// </summary>
        public static async Task<Tweakbox> Create(IReloadedHooks hooks, IReloadedHooksUtilities hooksUtilities,
            string modFolder)
        {
            var tweakBox = new Tweakbox();
            tweakBox.InitializeIoC(modFolder);
            tweakBox.Hooks = hooks;
            tweakBox.HooksUtilities = hooksUtilities;
            tweakBox.BlockInputsHook = Functions.GetInputs.Hook(tweakBox.BlockGameInputsIfEnabled).Activate();

            tweakBox.MenuBar = new MenuBar()
            {
                Menus = new List<MenuBarItem>()
                {
                    new MenuBarItem("Netplay", new List<IComponent>()
                    {
                        IoC.GetConstant<NetplayMenu>()
                    }),
                    new MenuBarItem("Tweaks", new List<IComponent>()
                    {
                        IoC.GetConstant<TweaksEditor>()
                    }),
                    new MenuBarItem("Editors", new List<IComponent>()
                    {
                        IoC.GetConstant<GearEditor>(),
                        IoC.GetConstant<PhysicsEditor>()
                    }),
                    new MenuBarItem("Debug", new List<IComponent>()
                    {
                        IoC.GetConstant<DemoWindow>(),
                        IoC.GetConstant<UserGuideWindow>(),
                        IoC.GetConstant<ShellTestWindow>(),
                        IoC.GetConstant<TaskTrackerWindow>(),
                        IoC.GetConstant<MemoryDebugWindow>(),
                        IoC.GetConstant<LogWindow>(),
                        IoC.GetConstant<RaceSettingsWindow>(),
                        IoC.GetConstant<DolphinDumperWindow>(),
                        IoC.GetConstant<LapCounterWindow>(),
                    })
                },
                Text = new List<string>()
                {
                    "F11: Show/Hide",
                    "F10: Enable/Disable Game Input"
                }
            };

            tweakBox.Hook = await ImguiHook.Create(tweakBox.Render);

            // Post-setup steps
            Shell.SetupImGuiConfig(modFolder);
            tweakBox.EnableCrashDumps();
            tweakBox.DisplayFirstTimeDialog();
            tweakBox.IsReady = true;
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

            // Initialize all configs.
            var configTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(IConfiguration).IsAssignableFrom(x) && !x.IsInterface);
            foreach (var type in configTypes)
                IoC.GetConstant(type);

            // Initialize all controllers.
            var controllerTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(IController).IsAssignableFrom(x) && !x.IsInterface);
            foreach (var type in controllerTypes)
                Controllers.Add(IoC.GetConstant<IController>(type));
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
            
            var localMachineKey = Environment.Is64BitOperatingSystem ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); 
            var key = localMachineKey.OpenSubKey(dumpsConfigRegkeyPath, false);
            if (key != null) 
                return;

            try
            {
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

            if (!IsEnabled) 
                return;

            // Update Menu Bar Text
            if (InputsEnabled)
                MenuBar.Text[1] = "F10: Disable Game Input";
            else
                MenuBar.Text[1] = "F10: Enable Game Input";

            // Render MenuBar and Menus
            MenuBar.Render();

            // Render Shell
            Shell.Render();
        }

        

        public void Suspend()
        {
            Hook.Disable();
            MenuBar.Suspend();
            Controllers.ForEach(x => x.Disable());
        }

        public void Resume()
        {
            Hook.Enable();
            MenuBar.Resume();
            Controllers.ForEach(x => x.Enable());
        }
    }
}
