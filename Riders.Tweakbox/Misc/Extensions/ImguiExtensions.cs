using DearImguiSharp;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Layout;
using System;
using System.Collections.Generic;

namespace Riders.Tweakbox.Misc.Extensions
{
    public static class ImguiExtensions
    {
        public static void RenderCenteredLabeledLink(this List<HorizontalCenterHelper> helpers, ref int centerIndex, string linkLabel, string urlLabel, string url)
        {
            RenderCentered(helpers, ref centerIndex, () =>
            {
                ImGui.BeginGroup();
                ImGui.Text($"{linkLabel}: ");
                ImGui.SameLine(0, 0);
                Hyperlink.CreateText(urlLabel, url, false);
                ImGui.EndGroup();
            });
        }

        public static void RenderCentered(this List<HorizontalCenterHelper> helpers, ref int index, Action render)
        {
            var neededHelpers = index - helpers.Count;
            for (int x = 0; x <= neededHelpers; x++)
                helpers.Add(new HorizontalCenterHelper());

            var helper = helpers[index];
            helper.Begin();
            render();
            helper.End();
            index++;
        }
    }
}
