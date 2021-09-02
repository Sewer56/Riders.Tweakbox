using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ByteSizeLib;
using DearImguiSharp;
using EnumsNET;
using Reloaded.Memory.Pointers;
using Riders.Netplay.Messages.Helpers;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Utilities;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Gameplay;
using Player = Sewer56.SonicRiders.API.Player;
using PlayerStruct = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public class InfoWindowController : IController
{
    private readonly InfoEditorConfig _config;
    private readonly FramePacingController _pacingController = IoC.Get<FramePacingController>();

    private List<InformationWindow> _infoWindows = new List<InformationWindow>();
    private SlidingBuffer<float> _cpuTimes = new SlidingBuffer<float>(30);
    private SlidingBuffer<float> _fpsTimes = new SlidingBuffer<float>(180);
    private SlidingBuffer<float> _potentialFpsTimes = new SlidingBuffer<float>(180);
    private SlidingBuffer<float> _renderTimes = new SlidingBuffer<float>(180);
    private SlidingBuffer<float> _frameTimes = new SlidingBuffer<float>(180);
    private SlidingBuffer<float> _heapValues = new SlidingBuffer<float>(180);

    private ImVec2 _graphSize = new ImVec2();
    private NetplayController _netplayController;

    public InfoWindowController(InfoEditorConfig config)
    {
        _config = config;
        _netplayController = IoC.GetSingleton<NetplayController>();
        Shell.AddCustom(RenderWidgets);
    }

    private unsafe bool RenderWidgets()
    {
        var data = _config.Data;
        var fps = _pacingController.Fps;

        // Check sufficient Windows have been Created
        int windowsNeeded = data.Widgets.Count - _infoWindows.Count;
        for (int x = 0; x < windowsNeeded; x++)
            _infoWindows.Add(new InformationWindow($"Info Widget No. {_infoWindows.Count}", Pivots.Pivot.TopRight, Pivots.Pivot.TopRight));

        // Set Font
        using var originalFont = ImGui.GetFont();
        ImGui.SetCurrentFont(Shell.MonoFont);

        // Collect Data
        var cpuUsage = _pacingController.CpuUsage;
        if (_cpuTimes.IsEmpty || cpuUsage != _cpuTimes.Back())
            _cpuTimes.PushBack(cpuUsage);

        _fpsTimes.PushBack((float)fps.StatFPS);
        _potentialFpsTimes.PushBack((float)fps.StatPotentialFPS);
        _renderTimes.PushBack((float)fps.StatRenderTime);
        _frameTimes.PushBack((float)fps.StatFrameTime);
        _heapValues.PushBack(Heap.GetUsedSize());

        // Render Widgets
        for (var x = 0; x < _config.Data.Widgets.Count; x++)
        {
            var config = _config.Data.Widgets[x];
            var window = _infoWindows[x];
            RenderWidget(window, config, fps);
        }

        // Restore Font
        ImGui.SetCurrentFont(originalFont);

        return true;
    }

    private void RenderWidget(InformationWindow window, InfoEditorConfig.WidgetConfig widgetConfig, FramePacer fps)
    {
        // Modify Window Title to Prevent ImGui Duplicates
        if (!widgetConfig.HasAnythingToShow())
            return;

        // Collect Data
        window.SetPivot(widgetConfig.Position, widgetConfig.Position);
        window.Size.X = widgetConfig.Width;
        window.Size.Y = widgetConfig.Height;
        _graphSize.X = widgetConfig.GraphWidth;
        _graphSize.Y = widgetConfig.GraphHeight;

        window.Begin();
        RenderWidgetContent(widgetConfig, fps);
        window.End();
    }

    private void RenderWidgetContent(InfoEditorConfig.WidgetConfig data, FramePacer fps)
    {
        RenderWidgetText(data, fps);
        RenderWidgetGraph(data, fps);
    }

    private unsafe void RenderWidgetGraph(InfoEditorConfig.WidgetConfig data, FramePacer fps)
    {
        if (data.ShowCpuGraph)
            ImGui.PlotLinesFloatPtr("CPU", ref _cpuTimes.Front(), _cpuTimes.Size, 0, null, 0, 100, _graphSize, sizeof(float));

        if (data.ShowFpsGraph)
            ImGui.PlotLinesFloatPtr("FPS", ref _fpsTimes.Front(), _fpsTimes.Size, 0, null, 0, fps.FPSLimit * 1.2f, _graphSize, sizeof(float));

        if (data.ShowMaxFpsGraph)
            ImGui.PlotLinesFloatPtr("Potential FPS", ref _potentialFpsTimes.Front(), _potentialFpsTimes.Size, 0, null, float.MaxValue, float.MaxValue, _graphSize, sizeof(float));

        if (data.ShowRenderTimeGraph)
            ImGui.PlotLinesFloatPtr("Render Time", ref _renderTimes.Front(), _renderTimes.Size, 0, null, 0, (float)fps.FrameTimeTarget, _graphSize, sizeof(float));

        if (data.ShowFrameTimeGraph)
            ImGui.PlotLinesFloatPtr("Frame Time", ref _frameTimes.Front(), _frameTimes.Size, 0, null, 0, (float)fps.FrameTimeTarget * 1.2f, _graphSize, sizeof(float));

        if (data.ShowHeapGraph)
            ImGui.PlotLinesFloatPtr("Heap", ref _heapValues.Front(), _heapValues.Size, 0, null, 0, Heap.GetHeapSize(), _graphSize, sizeof(float));
    }

    private unsafe void RenderWidgetText(InfoEditorConfig.WidgetConfig data, FramePacer fps)
    {
        if (data.ShowCpuNumber)
            ImGui.Text($"CPU: {_pacingController.CpuUsage:00.00}%%");

        if (data.ShowFpsNumber)
            ImGui.Text($"FPS: {fps.StatFPS:00.00}");

        if (data.ShowMaxFpsNumber)
            ImGui.Text($"Potential FPS: {fps.StatPotentialFPS:00.00}");

        if (data.ShowRenderTimeNumber)
            ImGui.Text($"Render: {fps.StatRenderTime:00.00}ms");

        if (data.ShowRenderTimePercent)
            ImGui.Text($"Render: {(fps.StatFrameTime / fps.StatRenderTime):00.00}%%");

        if (data.ShowFrameTimeNumber)
            ImGui.Text($"FrameTime: {fps.StatFrameTime:00.00}ms");

        if (data.ShowHeapNumber)
            ImGui.Text($"Heap: {ByteSize.FromBytes(Heap.GetUsedSize())}");

        if (data.ShowHeapPercent)
        {
            var percent = (Heap.GetUsedSize() / (float)Heap.GetHeapSize()) * 100f;
            ImGui.Text($"Heap: {percent:00.00}%%");
        }

        if (data.ShowPlayerPos != InfoEditorConfig.Player.None)
        {
            var values = Enums.GetValues<InfoEditorConfig.Player>();
            foreach (var value in values)
            {
                if (!data.ShowPlayerPos.HasAllFlags(value) || value == 0)
                    continue;

                var index = value.ToPlayerIndex();
                var playerPos = Player.Players[index].Position;
                ImGui.Text($"Pos [{index}]: ({playerPos.X:000.00000},{playerPos.Y:000.00000},{playerPos.Z:000.00000})");
            }
        }

        if (data.ShowRacePositions)
        {
            // Get all player info.
            var numRacers = *State.NumberOfRacers;
            Span<IndexedPlayer> playerPtrs = stackalloc IndexedPlayer[numRacers];
            for (int x = 0; x < numRacers; x++)
                playerPtrs[x] = new IndexedPlayer(x, Player.Players.Pointer + x);
            
            playerPtrs.Sort((x, y) =>
            {
                return x.Pointer->RacePosition.CompareTo(y.Pointer->RacePosition);
            });

            var stageStuff = *(UnknownStageDataStuff**)0x17E3E2C;
            if (stageStuff != (void*)0)
            {
                var perLapProgression = stageStuff->CheckpointProgressionPerLap * 100;
                var normalizedProgFirst = NormalizeProgression(playerPtrs[0].Pointer->CheckpointProgression, perLapProgression);
                var timer = *State.StageTimer;

                if (normalizedProgFirst > 0)
                {
                    // Check for Netplay
                    if (_netplayController.IsConnected())
                        ShowRacePositionsForNetplay(playerPtrs, normalizedProgFirst, timer.ToTimeSpan(), perLapProgression);
                    else
                        ShowRacePositionsForNormalRace(playerPtrs, normalizedProgFirst, timer.ToTimeSpan(), perLapProgression);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct UnknownStageDataStuff
    {
        /// <summary>
        /// Multiply by 100 to get scale used by player.
        /// </summary>
        [FieldOffset(0x14)]
        public float CheckpointProgressionPerLap;
    }

    private float NormalizeProgression(float progression, float perLapProgression)
    {
        // Normalize the bit after decimal to match scale of what is before it and rem.
        var afterDecimal = progression % 1;

        // Remove what's after decimal from progression.
        progression -= afterDecimal;

        // Normalize the part after decimal and add to progression.
        return progression + (afterDecimal * 1000) - perLapProgression;
    }

    private unsafe void ShowRacePositionsForNetplay(Span<IndexedPlayer> playerPtrs, float progressionFirst, TimeSpan timer, float perLapProgression)
    {
        var socket = _netplayController.Socket;
        var state  = socket.State;

        // Display
        for (var x = 0; x < playerPtrs.Length; x++)
        {
            var player = playerPtrs[x];
            var timerMultiplier = NormalizeProgression(player.Pointer->CheckpointProgression, perLapProgression) / progressionFirst;
            var timeBehind = (timer * timerMultiplier) - timer;

            var client = state.GetClientInfo(player.PlayerIndex, out int offset);
            if (client.NumPlayers > 1)
                ImGui.Text($"{x}. {client.Name}({offset}): {timeBehind.TotalSeconds:00.00}s");
            else
                ImGui.Text($"{x}. {client.Name}: {timeBehind.TotalSeconds:00.00}s");
        }
    }

    private unsafe void ShowRacePositionsForNormalRace(Span<IndexedPlayer> playerPtrs, float progressionFirst, TimeSpan timer, float perLapProgression)
    {
        // Display
        for (var x = 0; x < playerPtrs.Length; x++)
        {
            var player = playerPtrs[x];
            var timerMultiplier = NormalizeProgression(player.Pointer->CheckpointProgression, perLapProgression) / progressionFirst;
            var timeBehind = (timer * timerMultiplier) - timer;

            ImGui.Text($"{x}. Player [{player.PlayerIndex}]: {timeBehind.TotalSeconds:00.00}s");
        }
    }

    private unsafe struct IndexedPlayer
    {
        public int PlayerIndex;
        public PlayerStruct* Pointer;

        public IndexedPlayer(int playerIndex, PlayerStruct* pointer)
        {
            PlayerIndex = playerIndex;
            Pointer = pointer;
        }
    }
}
