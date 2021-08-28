using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using DearImguiSharp;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Services.Texture;
using Riders.Tweakbox.Services.Texture.Interfaces;
using Riders.Tweakbox.Services.Texture.Structs;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Controls.Extensions;
using Sewer56.Imgui.Shell.Interfaces;
using SharpDX.Direct3D9;
using static DearImguiSharp.ImGuiTableFlags;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;
namespace Riders.Tweakbox.Components.Tweaks;

public unsafe class TextureEditor : ComponentBase<TextureInjectionConfig>, IComponent
{
    /// <inheritdoc />
    public override string Name { get; set; } = "DirectX Texture Injection";

    private TextureInjectionController _injectionController;
    private TextureService _textureService;
    private List<TextureDictionaryBase> _dictionaries = new List<TextureDictionaryBase>();

    private ImageRenderer _imageRenderer = new ImageRenderer();

    private TextureCreationParameters _currentTexture;
    private int _currentImageIndex;
    private int _imageCount;
    private bool _showNonMipmapped;

    /// <inheritdoc />
    public TextureEditor(IO io, TextureInjectionController injectionController, TextureService textureService) : base(io, io.TextureConfigFolder, io.GetTextureConfigFiles, IO.JsonConfigExtension)
    {
        _injectionController = injectionController;
        _textureService = textureService;
    }

    /// <inheritdoc />
    public override unsafe void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            ProfileSelector.Render();
            RenderMenu();
        }

        ImGui.End();
    }

    private unsafe void RenderMenu()
    {
        ImGui.Separator();
        ImGui.TextWrapped("DirectX 9 Hash Based Texture Injection.");

        Hyperlink.CreateText($"Click here to learn more about custom textures.", "https://sewer56.dev/Riders.Tweakbox/textures/");
        ImGui.TextWrapped($"Note: Enable {nameof(LogCategory.TextureLoad)} and/or {nameof(LogCategory.TextureDump)} in your log configuration for notifications about when custom textures are loaded.");

        if (ImGui.TreeNodeStr("About"))
        {
            ImGui.TextWrapped("Dolphin Emulator style Hash Based Texture Injection.");
            ImGui.TextWrapped("I built this functionality to solve a few issues:\n" +
                              "- Duplicate textures across e.g. Gear x Character combinations.\n" +
                              "- Problems with menu texture scaling (texture resolution influences size).\n" +
                              "- Implementing the custom gear system.");
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Loaded Texture Viewer"))
        {
            ShowTextureMenu();
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Dump Textures"))
        {
            RenderDumpTextures();
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Texture Overrides"))
        {
            _dictionaries.Clear();
            _textureService.GetTextureDictionaries(_dictionaries);

            if (ImGui.TreeNodeStr("Textures"))
            {
                foreach (var dict in _dictionaries)
                foreach (var redirect in dict.Redirects)
                    ImGui.TextWrapped($"{redirect.Key} -> {redirect.Value.Path}");

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Animated Textures"))
            {
                foreach (var dict in _dictionaries)
                foreach (var redirect in dict.AnimatedRedirects)
                    ImGui.TextWrapped($"{redirect.Key} -> {redirect.Value.Folder}");

                ImGui.TreePop();
            }
            
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Debug"))
        {
            RenderDebug();
            ImGui.TreePop();
        }
    }

    private unsafe void ShowTextureMenu()
    {
        var contentRegionWidth = ImGui.GetWindowContentRegionWidth();
        var textures = _textureService.GetAllD3dTextures();

        if (textures.Length <= 0)
            return;

        ImGui.TextWrapped($"Current Texture: {_currentImageIndex:000}/{_imageCount:000}");

        // Display the table.
        const int tableWidth = 250;
        float remainingWidth = (contentRegionWidth - tableWidth);
        
        var textureTableSize = new ImVec2.__Internal() { x = tableWidth, y = -40 };
        const int tableFlags = (int)(ImGuiTableFlagsRowBg | ImGuiTableFlagsBorders | ImGuiTableFlagsNoBordersInBody | ImGuiTableFlagsScrollY | ImGuiTableFlagsContextMenuInBody);

        ImGui.BeginGroup();
        if (ImGui.__Internal.BeginTable("texture_table", 1, tableFlags, textureTableSize, 0))
        {
            // Create Headers
            // TODO: Re-add Type when they become relevant.
            ImGui.TableSetupColumn("Texture", 0, 0, 0);
            ImGui.TableSetupScrollFreeze(1, 1);

            // Show Headers
            ImGui.TableHeadersRow();

            // Render items
            int totalIndex = 0;
            foreach (var texture in textures)
            {
                // Setup
                var item = texture.TextureOut;
                bool isSelected = item == (_currentTexture == null ? (byte**)0 : _currentTexture.TextureOut);

                // Mipmap check.
                bool showTexture = _showNonMipmapped || _textureService.ShouldGenerateMipmap(texture.Hash);
                if (!showTexture)
                    continue;

                ImGui.PushID_Int(totalIndex);
                ImGui.TableNextRow(0, 0);
                int columnIndex = 0;

                // Name (Selectable)
                ImGui.TableSetColumnIndex(columnIndex++);
                var textureDesc = new Texture((IntPtr)(*item)).GetLevelDescription(0);
                if (ImGui.__Internal.SelectableBool($"{textureDesc.Width}x{textureDesc.Height} | {texture.Hash}", isSelected, (int)0, new ImVec2.__Internal() { x = 0, y = 0 }))
                {
                    _currentTexture = texture;
                    _currentImageIndex = totalIndex;
                }

                // Cleanup
                ImGui.PopID();
                totalIndex++;
            }

            _imageCount = totalIndex - 1;
            ImGui.EndTable();
        }

        if (ImGui.Checkbox("Show Non-Mipmapped Textures", ref _showNonMipmapped))
        {
            _currentImageIndex = 0;
            _currentTexture = null;
        }

        Tooltip.TextOnHover("Tweakbox uses non-mipmapped textures for the dummy textures it places inside the menus to support features such as custom gears.\n" +
                            "Normally it's not much use to view these textures as they're blank.");
        ImGui.EndGroup();

        // Render Current Item
        if (_currentTexture == null)
            return;

        RenderTexture(_currentTexture, remainingWidth);
    }

    private void RenderTexture(TextureCreationParameters currentImage, float remainingWidth)
    {
        // Setup
        const float spacing = 20;
        ImGui.SameLine(0, spacing);
        ImGui.BeginGroup();

        // Render Texture
        var texturePtr = (IntPtr)(*currentImage.TextureOut);
        var texture = new Texture(texturePtr);
        var desc = texture.GetLevelDescription(0);
        _imageRenderer.SetImageSize(new Vector2(desc.Width, desc.Height));
        _imageRenderer.Render(texturePtr);

        // Texture Details
        if (_textureService.TryGetInfo(currentImage.Hash, out var info))
        {
            ImGui.TextWrapped("This is an injected texture.");
            ImGui.TextWrapped($"Loaded From: {info.Path}");
            ImGui.TextWrapped($"Texture Type: {info.Type}");
        }

        // End
        ImGui.EndGroup();
    }

    private unsafe void RenderDebug()
    {
        if (ImGui.Button("Reload All Custom Textures", Constants.Zero))
        {
            var textures = _textureService.GetAllD3dTextures();
            foreach (var texture in textures)
                if (texture.IsCustomTexture)
                    _textureService.TryReloadCustomTexture(texture.Hash);
        }

        Tooltip.TextOnHover("[EXPERIMENTAL - BUT SHOULD BE SAFE TO USE]\n" +
                            "Reloads all injected custom textures; allowing for changes to your custom textures to be immediately seen.");

        if (ImGui.Button("Reload All Textures", Constants.Zero))
        {
            var textures = _textureService.GetAllD3dTextures();
            foreach (var texture in textures)
                _textureService.TryReloadTexture(texture.Hash);
        }

        Tooltip.TextOnHover("[EXPERIMENTAL - MAY CRASH]\n" +
                            "Reloads all textures used by the game; allowing for additional custom textures to be loaded without restarting the menu/stage.");

        if (ImGui.Button("Print All Textures", Constants.Zero))
        {
            var textures = _textureService.GetAllD3dTextures();
            foreach (var texture in textures)
                Log.WriteLine($"xxHash: {texture.Hash}, Pointer {(long)texture.NativePointer:X}, ppTexture: {(long)texture.TextureOut:X}");
        }

        Tooltip.TextOnHover("[FOR DEBUGGING] Prints details of all currently known used textures.");
    }

    private unsafe void RenderDumpTextures()
    {
        ImGui.Checkbox("Dump Textures", ref Config.Data.DumpTextures);
        Tooltip.TextOnHover($"Textures are dumped when they are loaded (e.g. stage load) as opposed to when they are shown (Dolphin Emu)\n" +
                            $"Texture Dump Directory:\n{Io.TextureDumpFolder}");

        if (ImGui.CollapsingHeaderBoolPtr("Texture Dump Options", ref Config.Data.DumpTextures, 0))
        {
            Reflection.MakeControlEnum((TextureInjectionConfig.DumpingMode*)Unsafe.AsPointer(ref Config.Data.DumpingMode), "Dumping Mode");
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
                    _injectionController.RemoveDuplicatesInCommon();
            }
        }
    }
}
