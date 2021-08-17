using System;
using System.Collections.Generic;
using Riders.Tweakbox.Configs.Json;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.Imgui.Utilities;
using static Riders.Tweakbox.Configs.InfoEditorConfig;

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

        public Player ShowPlayerPos = Player.None;

        public bool HasAnythingToShow()
        {
            return ShowFpsNumber || ShowFpsGraph
                                 || ShowFrameTimeNumber || ShowFrameTimeGraph
                                 || ShowMaxFpsNumber || ShowMaxFpsGraph
                                 || ShowCpuNumber || ShowCpuGraph
                                 || ShowRenderTimeNumber || ShowRenderTimeGraph || ShowRenderTimePercent
                                 || ShowHeapNumber || ShowHeapGraph || ShowHeapPercent
                                 || (ShowPlayerPos != Player.None);
        }
    }

    [Flags]
    public enum Player
    {
        None,
        PlayerOne = 1 << 0,
        PlayerTwo = 1 << 1,
        PlayerThree = 1 << 2,
        PlayerFour = 1 << 3,
        PlayerFive = 1 << 4,
        PlayerSix = 1 << 5,
        PlayerSeven = 1 << 6,
        PlayerEight = 1 << 7
    }
}

public static class InfoEditorConfigExtensions
{
    public static int ToPlayerIndex(this Player player)
    {
        return player switch
        {
            Player.None => -1,
            Player.PlayerOne => 0,
            Player.PlayerTwo => 1,
            Player.PlayerThree => 2,
            Player.PlayerFour => 3,
            Player.PlayerFive => 4,
            Player.PlayerSix => 5,
            Player.PlayerSeven => 6,
            Player.PlayerEight => 7,
            _ => -1
        };
    }
}