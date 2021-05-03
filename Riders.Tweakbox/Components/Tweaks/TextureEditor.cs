using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DearImguiSharp;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components.Tweaks
{
    public class TextureEditor : ComponentBase<TextureInjectionConfig>, IComponent
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "DirectX Texture Injection";

        /// <inheritdoc />
        public TextureEditor(IO io) : base(io, io.TextureConfigFolder, io.GetTextureConfigFiles, IO.JsonConfigExtension)
        {

        }

        /// <inheritdoc />
        public override void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                ProfileSelector.Render();

                ImGui.TextWrapped("Dolphin style hash based Texture Injection.");
                ImGui.Separator();
                ImGui.TextWrapped("Built to solve the problem of duplicate textures across e.g. Gear x Character combinations as well as solving menu texture scaling.");

                ImGui.Checkbox("Dump Textures", ref Config.Data.DumpTextures);
                Tooltip.TextOnHover($"Texture Dump Directory:\n{Io.TextureDumpFolder}");

                ImGui.Checkbox("Load Custom Textures", ref Config.Data.LoadTextures);
                Tooltip.TextOnHover("In order to load custom textures, add a `Tweakbox/Textures` folder to your Reloaded mod and place textures in PNG or DDS (Recommended) format inside.\n" +
                                    "Textures are loaded using the priority set in the Reloaded launcher (drag & drop) where bottom-most mod has the highest priority.");

                ImGui.Separator();
                ImGui.TextWrapped($"Note: This functionality is experimental.\n\nThere is no way to force reload textures live at this moment in time, you must wait for the game to load them. " +
                                  $"If you are unsure whether your textures are being loaded, enabled {nameof(LogCategory.TextureLoad)} and/or {nameof(LogCategory.TextureDump)} in your log configuration.");
            }

            ImGui.End();
        }
    }
}
