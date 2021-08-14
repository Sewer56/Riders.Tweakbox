using System;
using System.Collections.Generic;
using ByteSizeLib;
using DearImguiSharp;
using Riders.Netplay.Messages.Helpers;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Utilities;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.Controllers
{
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

        public InfoWindowController(InfoEditorConfig config)
        {
            _config = config;
            Shell.AddCustom(RenderWidgets);
        }

        private unsafe bool RenderWidgets()
        {
            var data = _config.Data;
            var fps  = _pacingController.Fps;

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
            _graphSize.X  = widgetConfig.GraphWidth;
            _graphSize.Y  = widgetConfig.GraphHeight;

            window.Begin();
            RenderWidgetContent(widgetConfig, fps);
            window.End();
        }

        private void RenderWidgetContent(InfoEditorConfig.WidgetConfig data, FramePacer fps)
        {
            RenderWidgetText(data, fps);
            RenderWidgetGraph(data, fps);
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
                var percent = (Heap.GetUsedSize() / (float) Heap.GetHeapSize()) * 100f;
                ImGui.Text($"Heap: {percent:00.00}%%");
            }
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
                ImGui.PlotLinesFloatPtr("Render Time", ref _renderTimes.Front(), _renderTimes.Size, 0, null, 0, (float) fps.FrameTimeTarget, _graphSize, sizeof(float));

            if (data.ShowFrameTimeGraph)
                ImGui.PlotLinesFloatPtr("Frame Time", ref _frameTimes.Front(), _frameTimes.Size, 0, null, 0, (float) fps.FrameTimeTarget * 1.2f, _graphSize, sizeof(float));

            if (data.ShowHeapGraph)
                ImGui.PlotLinesFloatPtr("Heap", ref _heapValues.Front(), _heapValues.Size, 0, null, 0, Heap.GetHeapSize(), _graphSize, sizeof(float));
        }
    }
}
