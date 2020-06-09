using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using DearImguiSharp;
using Reloaded.Hooks.Definitions;
using Reloaded.Imgui.Hook;
using Riders.Tweakbox.Components.FixesEditor;
using Riders.Tweakbox.Components.GearEditor;
using Riders.Tweakbox.Components.Imgui;
using Riders.Tweakbox.Components.Misc;
using Riders.Tweakbox.Components.Netplay;
using Riders.Tweakbox.Components.PhysicsEditor;
using Riders.Tweakbox.Definitions;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Utilities;
using Sewer56.SonicRiders.Functions;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace Riders.Tweakbox
{
    public class Tweakbox
    {
        /* Class Declarations */
        private ImguiHook _hook;
        private IReloadedHooks _hooks;
        private IReloadedHooksUtilities _hooksUtilities;
        
        private bool _inputsEnabled = true;
        private bool _isEnabled = true;
        private bool _isReady = false;
        private MenuBar _menuBar;
        private IHook<Functions.GetInputsFn> _blockInputsHook;

        /* Creation & Disposal */
        private Tweakbox(){}

        /// <summary>
        /// Creates a new instance of Riders Tweakbox.
        /// </summary>
        public static async Task<Tweakbox> Create(IReloadedHooks hooks, IReloadedHooksUtilities hooksUtilities,
            string modFolder)
        {
            var tweakBox = new Tweakbox();
            var imguiHook = await ImguiHook.Create(tweakBox.Render);
            IoC.Initialize(modFolder);
            tweakBox.SetupTheme(modFolder);
            tweakBox._hook = imguiHook;
            tweakBox._hooks = hooks;
            tweakBox._hooksUtilities = hooksUtilities;
            tweakBox._menuBar = new MenuBar()
            {
                Menus = new List<MenuBarItem>()
                {
                    new MenuBarItem("Netplay", new List<IComponent>()
                    {
                        IoC.GetConstant<NetplayMenu>()
                    }),
                    new MenuBarItem("Fixes", new List<IComponent>()
                    {
                        IoC.GetConstant<FixesEditor>()
                    }),
                    new MenuBarItem("Editors", new List<IComponent>()
                    {
                        IoC.GetConstant<GearEditor>(),
                        IoC.GetConstant<PhysicsEditor>()
                    }),
                    new MenuBarItem("Misc", new List<IComponent>()
                    {
                        IoC.GetConstant<DemoWindow>(),
                        IoC.GetConstant<UserGuideWindow>(),
                        IoC.GetConstant<ShellTestWindow>(),
                        IoC.GetConstant<TaskTrackerWindow>()
                    })
                },
                Text = new List<string>()
                {
                    "F11: Show/Hide",
                    "F10: Enable/Disable Game Input"
                }
            };

            tweakBox._blockInputsHook = Functions.GetInputs.Hook(tweakBox.BlockGameInputsIfEnabled).Activate();
            tweakBox._isReady = true;
            return tweakBox;
        }

        private int BlockGameInputsIfEnabled()
        {
            // Skips game controller input obtain function is menu is open.
            if (_inputsEnabled)
                return _blockInputsHook.OriginalFunction();

            return 0;
        }

        /* Implementation */
        private void Render()
        { 
            if (!_isReady)
                return;

            // This works because the keys sent to imgui in WndProc follow
            // the Windows key code order.
            if (ImGui.IsKeyPressed((int) Keys.F11, false))
                _isEnabled = !_isEnabled;

            if (ImGui.IsKeyPressed((int)Keys.F10, false))
                _inputsEnabled = !_inputsEnabled;

            if (!_isEnabled) 
                return;

            // Update Menu Bar Text
            if (_inputsEnabled)
                _menuBar.Text[1] = "F10: Disable Game Input";
            else
                _menuBar.Text[1] = "F10: Enable Game Input";

            // Render MenuBar and Menus
            _menuBar.Render();

            // Render Shell
            Shell.Render();
        }

        public void Suspend()
        {
            _hook.Disable();
            _menuBar.Suspend();
        }

        public void Resume()
        {
            _hook.Enable();
            _menuBar.Resume();
        }

        // Setup
        private unsafe void SetupTheme(string modFolder)
        {
            var io = ImGui.GetIO();
            io.BackendFlags |= (int)ImGuiBackendFlags.ImGuiBackendFlagsHasGamepad;
            io.BackendFlags |= (int)ImGuiBackendFlags.ImGuiBackendFlagsHasSetMousePos;
            io.ConfigFlags |= (int)ImGuiConfigFlags.ImGuiConfigFlagsNavEnableGamepad;
            io.ConfigFlags |= (int)ImGuiConfigFlags.ImGuiConfigFlagsNavEnableKeyboard;
            io.ConfigFlags &= ~(int)ImGuiConfigFlags.ImGuiConfigFlagsNavEnableSetMousePos;
            io.IniFilename = Path.Combine(modFolder, "imgui.ini");

            var fontPath = Path.Combine(modFolder, "Assets/Fonts/Ruda-Bold.ttf");
            var font = ImGui.ImFontAtlasAddFontFromFileTTF(io.Fonts, fontPath, 15.0f, null, ref Constants.NullReference<ushort>());
            if (font != null)
                io.FontDefault = font;

            var style = ImGui.GetStyle();
            style.FrameRounding = 4.0f;
            style.WindowBorderSize = 0.0f;
            style.PopupBorderSize = 0.0f;
            style.GrabRounding = 4.0f;

            var colors = style.Colors;
            colors[(int)ImGuiCol.ImGuiColText] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColTextDisabled] = new Vector4(0.73f, 0.75f, 0.74f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColWindowBg] = new Vector4(0.09f, 0.09f, 0.09f, 0.94f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColPopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColBorder] = new Vector4(0.20f, 0.20f, 0.20f, 0.50f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColBorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColFrameBg] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColFrameBgHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.40f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColFrameBgActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.67f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColTitleBg] = new Vector4(0.47f, 0.22f, 0.22f, 0.67f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColTitleBgActive] = new Vector4(0.47f, 0.22f, 0.22f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColTitleBgCollapsed] = new Vector4(0.47f, 0.22f, 0.22f, 0.67f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColMenuBarBg] = new Vector4(0.34f, 0.16f, 0.16f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColCheckMark] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColSliderGrab] = new Vector4(0.71f, 0.39f, 0.39f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColSliderGrabActive] = new Vector4(0.84f, 0.66f, 0.66f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColButton] = new Vector4(0.47f, 0.22f, 0.22f, 0.65f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColButtonHovered] = new Vector4(0.71f, 0.39f, 0.39f, 0.65f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColButtonActive] = new Vector4(0.20f, 0.20f, 0.20f, 0.50f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColHeader] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColHeaderHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.65f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColHeaderActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColSeparator] = new Vector4(0.43f, 0.43f, 0.50f, 0.50f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColSeparatorHovered] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColSeparatorActive] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColResizeGrip] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColResizeGripHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColResizeGripActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColTab] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColTabHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColTabActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColTabUnfocused] = new Vector4(0.07f, 0.10f, 0.15f, 0.97f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColTabUnfocusedActive] = new Vector4(0.14f, 0.26f, 0.42f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColPlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColPlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColPlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColPlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColTextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColDragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColNavHighlight] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColNavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColNavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f).ToImVec();
            colors[(int)ImGuiCol.ImGuiColModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f).ToImVec();
            style.Colors = colors;
        }

    }
}
