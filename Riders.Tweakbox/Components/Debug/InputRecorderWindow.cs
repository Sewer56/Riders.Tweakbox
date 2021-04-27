using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using DearImguiSharp;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory;
using Reloaded.Memory.Streams;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Input;
using Constants = Sewer56.Imgui.Misc.Constants;

namespace Riders.Tweakbox.Components.Debug
{
    public class InputRecorderWindow : ComponentBase
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Input Recorder Window";
        
        // Recording Properties
        private bool _recordingStarted;
        private Stopwatch _recordingDuration = new Stopwatch();
        private List<PlayerInput> _inputs = new List<PlayerInput>();

        private IHook<Functions.CdeclReturnIntFn> _getInputHook;
        
        // I/O
        private IO _io;

        public InputRecorderWindow(IO io)
        {
            _io = io;
            _getInputHook = Functions.GetInputs.Hook(OnGetInputs).Activate();
        }

        /// <inheritdoc />
        public override void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                if (_recordingStarted)
                {
                    ImGui.TextWrapped($"Current Num. Frames: {_inputs.Count / 8}");
                    ImGui.TextWrapped($"Time Recording: {_recordingDuration.Elapsed.ToString(@"hh\:mm\:ss")}");
                    if (ImGui.Button("Stop Recording", Constants.ButtonSize))
                        StopRecording();
                }
                else
                {
                    if (ImGui.Button("Start Recording", Constants.ButtonSize))
                        StartRecording();
                }

            }

            ImGui.End();
        }

        private void StartRecording()
        {
            _inputs.Clear();
            _recordingDuration.Restart();
            _recordingStarted = true;
        }

        private void StopRecording()
        {
            _recordingStarted = false;
            var fileName = DateTime.UtcNow.ToString("yyyy.MM.dd.T.HH.mm.ss");
            var filePath = Path.Combine(_io.InputRecordingFolder, fileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            var numImputs = _inputs.Count;
            for (int x = 0; x < numImputs; x++)
                fileStream.Write(_inputs[x]);

            _inputs.Clear();
            _recordingDuration.Reset();
        }

        private unsafe int OnGetInputs()
        {
            var result = _getInputHook.OriginalFunction();

            if (_recordingStarted)
            {
                // Record Frame
                for (int x = 0; x < Player.MaxNumberOfPlayers; x++)
                    _inputs.Add(*Player.Players[x].PlayerInput);
            }

            return result;
        }
    }
}