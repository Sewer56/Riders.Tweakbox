using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DearImguiSharp;
using Sewer56.Imgui.Misc;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.Components.Debug
{
    public class LapCounterWindow : ComponentBase
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Lap Counter Viewer";

        /// <inheritdoc />
        public override void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                for (int x = 0; x < Riders.Netplay.Messages.Misc.Constants.MaxNumberOfPlayers; x++)
                    ImGui.TextWrapped($"Player {x}: {Player.Players[x].LapCounter}");
            }

            ImGui.End();
        }
    }
}
