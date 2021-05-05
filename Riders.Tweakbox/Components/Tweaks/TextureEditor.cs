using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Tweaks
{
    public class TextureEditor : ComponentBase<TextureInjectionConfig>, IComponent
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "DirectX Texture Injection";

        private TextureController _controller;

        /// <inheritdoc />
        public TextureEditor(IO io, TextureController controller) : base(io, io.TextureConfigFolder, io.GetTextureConfigFiles, IO.JsonConfigExtension)
        {
            _controller = controller;
        }

        /// <inheritdoc />
        public override unsafe void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                ProfileSelector.Render();

                ImGui.TextWrapped("Dolphin style hash based Texture Injection.");
                ImGui.Separator();
                ImGui.TextWrapped("Built to solve the problem of duplicate textures across e.g. Gear x Character combinations as well as solving menu texture scaling.");

                ImGui.Checkbox("Dump Textures", ref Config.Data.DumpTextures);
                Tooltip.TextOnHover($"Textures are dumped when they are loaded (e.g. stage load) as opposed to when they are shown (Dolphin Emu)\n" +
                                    $"Texture Dump Directory:\n{Io.TextureDumpFolder}");

                ImGui.Checkbox("Load Custom Textures", ref Config.Data.LoadTextures);
                Tooltip.TextOnHover("In order to load custom textures, create a new Reloaded mod and add `Tweakbox/Textures` folder to the mod\n." +
                                    "Place textures in PNG or DDS format inside. Textures are loaded using the priority set in the Reloaded\n" +
                                    "launcher (drag & drop) where bottom-most mod has the highest priority.");

                if (ImGui.CollapsingHeaderBoolPtr("Texture Dump Options", ref Config.Data.DumpTextures, 0))
                {
                    Reflection.MakeControlEnum((TextureInjectionConfig.DumpingMode*) Unsafe.AsPointer(ref Config.Data.DumpingMode), "Dumping Mode");
                    Tooltip.TextOnHover($"Only New: Only dumps textures if they are not already in the dump folder.\n" +
                                        $"Deduplicate: Removes duplicates in folders by putting them in a common folder at \n" +
                                        $"{Io.TextureDumpCommonFolder}");

                    if (Config.Data.DumpingMode == TextureInjectionConfig.DumpingMode.Deduplicate)
                    {
                        Reflection.MakeControl(ref Config.Data.DeduplicationMaxFiles, "Maximum Duplicates", 0.01f);
                        Tooltip.TextOnHover($"Maximum number of duplicates before moving to common folder.\n" +
                                            $"This is set to 2 by default because stage pairs (e.g. Metal City, Night Chase) often have shared textures.");

                        bool remove = ImGui.Button("Remove Duplicates in Common Folder", Constants.ButtonSize);
                        Tooltip.TextOnHover($"Removes items in the Common Folder ({Io.TextureDumpCommonFolder})\n" +
                                            $"that are duplicates of items outside of the common folder.");

                        if (remove)
                            _controller.RemoveDuplicatesInCommon();
                    }
                }

                ImGui.Separator();
                ImGui.TextWrapped($"Note: This functionality is experimental.\n\nTextures are injected at time of load, there is no force reload functionality currently (e.g. you must exit and re-enter stage to take effect). " +
                                  $"If you are unsure whether your textures are being loaded, enabled {nameof(LogCategory.TextureLoad)} and/or {nameof(LogCategory.TextureDump)} in your log configuration.");
            }

            ImGui.End();
        }
    }
}
