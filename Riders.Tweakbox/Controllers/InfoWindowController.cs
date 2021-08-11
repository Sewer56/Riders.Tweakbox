using DearImguiSharp;
using Riders.Netplay.Messages.Helpers;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Utilities;

namespace Riders.Tweakbox.Controllers
{
    public class InfoWindowController : IController
    {
        private readonly InfoEditorConfig _config;
        private readonly FramePacingController _pacingController = IoC.Get<FramePacingController>();
        private static InformationWindow _infoWindow = new InformationWindow("Information Window", Pivots.Pivot.TopRight, Pivots.Pivot.TopRight);

        private SlidingBuffer<float> _cpuTimes = new SlidingBuffer<float>(30);
        private SlidingBuffer<float> _fpsTimes = new SlidingBuffer<float>(180);
        private SlidingBuffer<float> _potentialFpsTimes = new SlidingBuffer<float>(180);
        private SlidingBuffer<float> _renderTimes = new SlidingBuffer<float>(180);
        private SlidingBuffer<float> _frameTimes = new SlidingBuffer<float>(180);

        private ImVec2 _graphSize = new ImVec2();

        public InfoWindowController(InfoEditorConfig config)
        {
            _config = config;
            _config.Data.AddPropertyUpdatedHandler(OnPropertyUpdated);
            Shell.AddCustom(RenderInfoWindow);
        }

        private void OnPropertyUpdated(string propertyname)
        {
            var data = _config.Data;
            switch (propertyname)
            {
                case nameof(data.Position):
                    _infoWindow.SetPivot(data.Position, data.Position);
                    break;
            }
        }

        private bool RenderInfoWindow()
        {
            var data = _config.Data;
            var fps = _pacingController.Fps;

            if (!data.HasAnythingToShow())
                return true;

            // Collect Data
            var cpuUsage = _pacingController.CpuUsage;
            if (_cpuTimes.IsEmpty || cpuUsage != _cpuTimes.Back())
                _cpuTimes.PushBack(cpuUsage);
            
            _fpsTimes.PushBack((float) fps.StatFPS);
            _potentialFpsTimes.PushBack((float) fps.StatPotentialFPS);
            _renderTimes.PushBack((float) fps.StatRenderTime);
            _frameTimes.PushBack((float) fps.StatFrameTime);

            // Set Font
            using var originalFont = ImGui.GetFont();
            ImGui.SetCurrentFont(Shell.MonoFont);

            _infoWindow.Size.X = data.Width;
            _infoWindow.Size.Y = data.Height;
            _graphSize.X = data.GraphWidth;
            _graphSize.Y = data.GraphHeight;

            _infoWindow.Begin();
            RenderInfoWindowContent(data, fps);
            _infoWindow.End();

            // Restore Font
            ImGui.SetCurrentFont(originalFont);

            return true;
        }

        private void RenderInfoWindowContent(InfoEditorConfig.Internal data, FramePacer fps)
        {
            if (data.ShowCpuNumber)
                ImGui.Text($"CPU: {_pacingController.CpuUsage:00.00}%%");

            if (data.ShowFpsNumber)
                ImGui.Text($"FPS: {fps.StatFPS:00.00}");

            if (data.ShowMaxFpsNumber)
                ImGui.Text($"Potential FPS: {fps.StatPotentialFPS:00.00}");

            if (data.ShowRenderTimeNumber)
                ImGui.Text($"Render Time: {fps.StatRenderTime:00.00}ms");

            if (data.ShowRenderTimePercent)
                ImGui.Text($"Render Time: {(fps.StatFrameTime / fps.StatRenderTime):00.00}%%");

            if (data.ShowFrameTimeNumber)
                ImGui.Text($"FrameTime: {fps.StatFrameTime:00.00}ms");

            if (data.ShowCpuGraph)
                ImGui.PlotLinesFloatPtr("CPU", ref _cpuTimes.Front(), _cpuTimes.Size, 0, null, 0, 100, _graphSize, sizeof(float));

            if (data.ShowFpsGraph)
                ImGui.PlotLinesFloatPtr("FPS", ref _fpsTimes.Front(), _fpsTimes.Size, 0, null, 0, fps.FPSLimit * 1.2f, _graphSize, sizeof(float));

            if (data.ShowMaxFpsGraph)
                ImGui.PlotLinesFloatPtr("Potential FPS", ref _potentialFpsTimes.Front(), _potentialFpsTimes.Size, 0, null, float.MaxValue, float.MaxValue, _graphSize, sizeof(float));

            if (data.ShowRenderTimeGraph)
                ImGui.PlotLinesFloatPtr("Render Time", ref _renderTimes.Front(), _renderTimes.Size, 0, null, 0, (float) fps.FrameTimeTarget, _graphSize, sizeof(float));

            if (data.ShowFrameTimeGraph)
                ImGui.PlotLinesFloatPtr("Frame Time", ref _frameTimes.Front(), _frameTimes.Size, 0, null, 0, (float)fps.FrameTimeTarget * 1.2f, _graphSize, sizeof(float));
        }
    }
}
