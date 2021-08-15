using System.Collections.Generic;
using Riders.Tweakbox.Configs.Json;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.Imgui.Utilities;
namespace Riders.Tweakbox.Configs;

public class InfoEditorConfig : JsonConfigBase<InfoEditorConfig, InfoEditorConfig.Internal>
{
    public class Internal : NotifyPropertyChangedBase
    {
        public List<WidgetConfig> Widgets = new List<WidgetConfig>() { new WidgetConfig(), new WidgetConfig() };
    }

    public class WidgetConfig : NotifyPropertyChangedBase
    {
        public Pivots.Pivot Position = Pivots.Pivot.TopRight;
        public int Width = 0;
        public int Height = 0;

        public int GraphWidth = 0;
        public int GraphHeight = 70;

        public bool ShowFpsNumber = false;
        public bool ShowFpsGraph = false;

        public bool ShowFrameTimeNumber = false;
        public bool ShowFrameTimeGraph = false;

        public bool ShowRenderTimeNumber = false;
        public bool ShowRenderTimeGraph = false;
        public bool ShowRenderTimePercent = false;

        public bool ShowMaxFpsNumber = false;
        public bool ShowMaxFpsGraph = false;

        public bool ShowCpuNumber = false;
        public bool ShowCpuGraph = false;

        public bool ShowHeapPercent = false;
        public bool ShowHeapNumber = false;
        public bool ShowHeapGraph = false;

        public bool HasAnythingToShow()
        {
            return ShowFpsNumber || ShowFpsGraph
                                 || ShowFrameTimeNumber || ShowFrameTimeGraph
                                 || ShowMaxFpsNumber || ShowMaxFpsGraph
                                 || ShowCpuNumber || ShowCpuGraph
                                 || ShowRenderTimeNumber || ShowRenderTimeGraph || ShowRenderTimePercent
                                 || ShowHeapNumber || ShowHeapGraph || ShowHeapPercent;
        }
    }
}
