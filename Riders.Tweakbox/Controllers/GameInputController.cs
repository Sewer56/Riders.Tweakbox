using DearImguiSharp;
using Riders.Tweakbox.Controllers.Interfaces;
using System;
using Riders.Controller.Hook.Interfaces;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Configs;
using static DearImguiSharp.ImGuiFocusedFlags;

namespace Riders.Tweakbox.Controllers;

public class GameInputController : IController
{
    private bool _inputBlocking;
    private IControllerHook _controllerHook;
    private Logger _log = new Logger(LogCategory.Default);
    private TweakboxConfig _config;

    public GameInputController(IControllerHook hook, Tweakbox tweakbox, TweakboxConfig config)
    {
        tweakbox.OnFrame += OnFrame;
        _controllerHook = hook;
        _config = config;
    }

    private void OnFrame()
    {
        var data = _config.Data;

        // Toggle Block State.
        switch (data.InputMode)
        {
            case TweakboxConfig.GameInput.Always:
                _controllerHook.EnableInputs = true;
                break;
            case TweakboxConfig.GameInput.Toggle:
            {
                if (ImGui.IsKeyPressed((int)data.InputToggleKey, false))
                {
                    _inputBlocking = !_inputBlocking;
                    var blockState = _inputBlocking ? "Enabled" : "Disabled";
                    _log.WriteLine($"Input Blocing Is Now {blockState}");
                }

                _controllerHook.EnableInputs = !_inputBlocking;
                break;
            }
            case TweakboxConfig.GameInput.WhenNoWindowActive:
            {
                _controllerHook.EnableInputs = !ImGui.IsWindowFocused((int)ImGuiFocusedFlagsAnyWindow);
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
