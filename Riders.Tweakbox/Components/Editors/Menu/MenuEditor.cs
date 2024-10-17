using System;
using System.Runtime.CompilerServices;
using DearImguiSharp;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Controllers.MenuEditor;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.Parser.Menu.Metadata;
using Sewer56.SonicRiders.Parser.Menu.Metadata.Enums;
using Sewer56.SonicRiders.Parser.Menu.Metadata.Structs;
using Sewer56.SonicRiders.Parser.Menu.Metadata.Structs.Frames;
using Sewer56.SonicRiders.Parser.Menu.Metadata.Structs.Helpers;
using Sewer56.SonicRiders.Structures.Misc;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Editors.Menu;

public class MenuEditor : ComponentBase, IComponent
{
    public override string Name { get; set; } = "Menu Editor";

    private MenuEditorController _menuEditorController = IoC.Get<MenuEditorController>();
    private bool _showAllProperties = false;

    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            RenderMenuEditor();
        }

        ImGui.End();
    }

    private unsafe void RenderMenuEditor()
    {
        int x = 0;
        ImGui.Checkbox("Show All Properties", ref _showAllProperties);

        foreach (var item in _menuEditorController.GetAllItems())
        {
            var name = GetMenuName(item.Key.MetadataFilePtrPtr);
            ImGui.PushID_Int(x);

            if (ImGui.CollapsingHeaderTreeNodeFlags(name, 0))
                RenderMenuEditorItem(item.Value);

            ImGui.PopID();
            x++;
        }
    }

    private unsafe void RenderMenuEditorItem(InMemoryMenuMetadata menuMetadata)
    {
        Reflection.MakeControl(&menuMetadata.Header->ResolutionX, nameof(MetadataHeader.ResolutionX));
        Reflection.MakeControl(&menuMetadata.Header->ResolutionY, nameof(MetadataHeader.ResolutionY));
        Reflection.MakeControl(&menuMetadata.Header->MaybeFramerate, nameof(MetadataHeader.MaybeFramerate));
        ImGui.DragScalar(nameof(MetadataHeader.AnimationType1Offset), (int)ImGuiDataType.U8, (IntPtr)(&menuMetadata.Header->AnimationType1Offset), 1.0F, IntPtr.Zero, IntPtr.Zero, null, (int)ImGuiSliderFlags.NoInput);

        ImGui.Text($"Entry Header Ptr: {((nint)menuMetadata.ObjectSectionHeader):X}");
        ImGui.SameLine(0, Constants.Spacing);
        ImGui.Text($"Texture Header Ptr: {((nint)menuMetadata.TextureIdHeader):X}");

        if (ImGui.TreeNodeStr("Objects"))
        {
            ImGui.Text($"Total Objects: {((nint)menuMetadata.ObjectSectionHeader->NumObjects):X}");
            ImGui.Text($"Total Section Size: {((nint)menuMetadata.ObjectSectionHeader->TotalSectionSize):X}");

            for (int x = 0; x < menuMetadata.Objects.Count; x++)
                RenderObject(x, menuMetadata);

            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Textures"))
        {
            ImGui.Text($"Total Entries: {((nint)menuMetadata.TextureIdHeader->NumTextures):X}");
            ImGui.Text($"Unknown: {((nint)menuMetadata.TextureIdHeader->unknown):X}");

            ImGui.TreePop();
        }
    }

    private unsafe void RenderObject(int entryIndex, InMemoryMenuMetadata itemValue)
    {
        if (ImGui.TreeNodeStr($"Object {entryIndex}"))
        {
            var entry = itemValue.Objects[entryIndex].Pointer;
            ImGui.Text($"Layer Count: {(entry->LayerCount):X}");
            ImGui.Text($"Object Size: {(entry->ObjectSize):X}");

            var actionLayers = itemValue.ActionLayers[entryIndex];
            var layers = itemValue.Layers[entryIndex];

            for (int x = 0; x < actionLayers.Count; x++)
            {
                var actionLayerPtr = actionLayers[x].Pointer;
                if (ImGui.TreeNodeStr($"Action Layer [{x}]"))
                {
                    RenderActionLayer(actionLayerPtr);
                }
            }

            for (int x = 0; x < layers.Count; x++)
            {
                var layer = layers[x].Pointer;

                if (ImGui.TreeNodeStr($"Layer [{x}]"))
                {
                    RenderLayer(layer, itemValue.Header);
                    ImGui.TreePop();
                }
            }
            
            if (ImGui.TreeNodeStr($"Layer Offsets"))
            {
                for (int x = 0; x < entry->LayerCount; x++)
                {
                    var subEntry = entry->GetLayerPointer(entry, x);
                    ImGui.Text($"Offset: {((nint)subEntry):X}");
                }

                ImGui.TreePop();
            }

            ImGui.TreePop();
        }
    }

    private unsafe void RenderActionLayer(ActionLayer* layer)
    {
        Reflection.MakeControl(&layer->IsEnabled, nameof(ActionLayer.IsEnabled), 0.1f, 0);
        Reflection.MakeControl(&layer->Unk_1, nameof(ActionLayer.Unk_1), 0.1f, 0);
        if (layer->IsEnabled < 1)
            return;

        Reflection.MakeControl(&layer->DurationOfLongestAnimation, nameof(ActionLayer.DurationOfLongestAnimation), 0.1f, 0);
        Reflection.MakeControl(&layer->UnknownFlag, nameof(ActionLayer.UnknownFlag), 0.1f, 0);
        Reflection.MakeControl(&layer->Unk_4, nameof(ActionLayer.Unk_4), 0.1f, 0);
        Reflection.MakeControl(&layer->Unk_5, nameof(ActionLayer.Unk_5), 0.1f, 0);
    }

    private unsafe void RenderLayer(Layer* layer, MetadataHeader* header)
    {
        int zero   = 0;
        short step = 1;
        int max  = 0xFF;

        ImGui.PushItemWidth(ImGui.GetFontSize() * -12);
        ImGui.TextWrapped($"Address: {((nint)layer):X}");

        // TODO: Add/Remove keyframes on drag.
        Reflection.MakeControl(&layer->NumKeyframes, nameof(Layer.NumKeyframes), 0.1f, 0);
        ImGui.InputScalar(nameof(Layer.NumBytes), (int)ImGuiDataType.S16, (IntPtr)(&layer->NumBytes), (IntPtr)(&step), IntPtr.Zero, "%04X", (int)(ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.ReadOnly));

        Reflection.MakeControl((short*)&layer->KeyframeType, nameof(Layer.KeyframeType));
        Reflection.MakeControlEnum(&layer->KeyframeType, $"{nameof(Layer.KeyframeType)} (Known)");

        Reflection.MakeControl(&layer->MaybeAnimationDurationFrames, nameof(Layer.MaybeAnimationDurationFrames), 0.1f, 0);
        Reflection.MakeControl(&layer->Unknown_1_0, nameof(Layer.Unknown_1_0));
        Reflection.MakeControl(&layer->Unknown_1_1, nameof(Layer.Unknown_1_1));
        Reflection.MakeControl(&layer->TextureIndex, nameof(Layer.TextureIndex), 0.1f, 0, (short)header->TextureIndicesPtr->NumTextures);
        
        ImGui.InputScalar(nameof(Layer.Flags), (int)ImGuiDataType.S16, (IntPtr)(&layer->Flags), (IntPtr)(&step), IntPtr.Zero, "%04X", (int)ImGuiInputTextFlags.CharsHexadecimal);
        Reflection.MakeControlEnum(&layer->Flags, nameof(Layer.Flags));

        Reflection.MakeControl(&layer->Width, nameof(Layer.Width), 0.1f, 0);
        Reflection.MakeControl(&layer->Height, nameof(Layer.Height), 0.1f, 0);

        Reflection.MakeControl(&layer->Unknown_SomeTimesOffsetX, nameof(Layer.Unknown_SomeTimesOffsetX));
        Reflection.MakeControl(&layer->Unknown_SomeTimesOffsetY, nameof(Layer.Unknown_SomeTimesOffsetY));

        Reflection.MakeControl(&layer->OffsetX, nameof(Layer.OffsetX));
        Reflection.MakeControl(&layer->OffsetY, nameof(Layer.OffsetY));

        Reflection.MakeControl(&layer->Unknown_2, nameof(Layer.Unknown_2));

        ImGuiExtensions.RenderColourPickerForAbgr(ref Unsafe.AsRef<ColorABGR>(&layer->ColorTopLeft), nameof(Layer.ColorTopLeft));
        ImGuiExtensions.RenderColourPickerForAbgr(ref Unsafe.AsRef<ColorABGR>(&layer->ColorBottomLeft), nameof(Layer.ColorBottomLeft));
        ImGuiExtensions.RenderColourPickerForAbgr(ref Unsafe.AsRef<ColorABGR>(&layer->ColorBottomRight), nameof(Layer.ColorBottomRight));
        ImGuiExtensions.RenderColourPickerForAbgr(ref Unsafe.AsRef<ColorABGR>(&layer->ColorTopRight), nameof(Layer.ColorTopRight));

        // Render Keyframes.
        Span<BlittablePointer<Keyframe>> keyFrameSpan = stackalloc BlittablePointer<Keyframe>[layer->NumKeyframes];
        keyFrameSpan = Layer.GetKeyFrames(layer, header, keyFrameSpan);

        for (int x = 0; x < keyFrameSpan.Length; x++)
        {
            if (ImGui.TreeNodeStr($"Keyframe {x}"))
            {
                RenderKeyframe(keyFrameSpan[x].Pointer, layer, header);
                ImGui.TreePop();
            }
        }

        ImGui.PopItemWidth();
    }

    private unsafe void RenderKeyframe(Keyframe* keyFrame, Layer* layer, MetadataHeader* header)
    {
        Reflection.MakeControlEnum(&keyFrame->KeyframeType, nameof(Keyframe.KeyframeType));
        Reflection.MakeControl(&keyFrame->AnimationActivationPointFrames, nameof(Keyframe.AnimationActivationPointFrames), 0.1f, 0);
        Reflection.MakeControl(&keyFrame->NumberOfChangedProperties, nameof(Keyframe.NumberOfChangedProperties), 0.1f, 0);
        Reflection.MakeControl(&keyFrame->NumberOfBytesDivBy4, nameof(Keyframe.NumberOfBytesDivBy4), 0.1f, 0);

        // Assumes max 64 items.
        Span<DataHeaderWrapper> keyFrameItems = stackalloc DataHeaderWrapper[64];
        keyFrameItems = keyFrame->GetData(keyFrame, layer, header, keyFrameItems);
        foreach (var item in keyFrameItems)
        {
            switch (item.DataType)
            {
                case (short)KeyframeDataType.Color:
                    RenderColor((Color*)item.DataPtr);
                    break;

                default:
                    RenderUnknown(item);
                    break;
            }   
        }
    }

    private unsafe void RenderUnknown(DataHeaderWrapper item)
    {
        if (!ImGui.TreeNodeStr($"Unknown Type: {item.DataType}, Num Bytes: {item.NumBytes}, 0x{(nint)item.DataPtr:X}"))
            return;

        int* startAddress = (int*)(item.DataPtr);
        int* endAddress = (int*)(item.DataPtr + item.NumBytes);
        int* currentAddress = startAddress;
        int id = 0;
        const int spacing = 10;

        uint min = 0;
        uint max = uint.MaxValue;

        while (currentAddress < endAddress)
        {
            var offset = (nint)currentAddress - (nint)startAddress;

            // Render current address.
            // 4 byte
            ImGuiExtensions.RenderLabel($"4 Bytes (0x{offset:X2})", spacing);
            ImGui.PushID_Int(id++);
            ImGui.DragScalar("", (int)ImGuiDataType.U32, (IntPtr)(currentAddress), 0.1f, (IntPtr)(&min), (IntPtr)(&max), $"{*currentAddress:X8}", (int)1);
            ImGui.PopID();

            ImGuiExtensions.RenderLabel($"2 Bytes (0x{offset:X2})", spacing);
            ImGuiExtensions.RenderIntAsShortsHex(currentAddress, spacing, ref id);

            ImGuiExtensions.RenderLabel($"1 Bytes (0x{offset:X2})", spacing);
            ImGuiExtensions.RenderIntAsBytesHex(currentAddress, spacing, ref id);

            currentAddress++;
        }

        ImGui.TreePop();
    }

    private unsafe void RenderColor(Color* color)
    {
        if (!ImGui.TreeNodeStr($"Color"))
            return;

        ImGuiExtensions.RenderColourPickerForAbgr(ref Unsafe.AsRef<ColorABGR>(&color->TopLeft), nameof(Color.TopLeft));
        ImGuiExtensions.RenderColourPickerForAbgr(ref Unsafe.AsRef<ColorABGR>(&color->BottomLeft), nameof(Color.BottomLeft));
        ImGuiExtensions.RenderColourPickerForAbgr(ref Unsafe.AsRef<ColorABGR>(&color->BottomRight), nameof(Color.BottomRight));
        ImGuiExtensions.RenderColourPickerForAbgr(ref Unsafe.AsRef<ColorABGR>(&color->TopRight), nameof(Color.TopRight));

        ImGui.TreePop();
    }

    private unsafe string GetMenuName(byte** keyMetadataFilePtrPtr)
    {
        if (Sewer56.SonicRiders.API.Menu.LayoutPointerToMenuEntry.TryGetValue((nint)keyMetadataFilePtrPtr, out var value))
            return value.FriendlyName;

        return $"0x{((nint)keyMetadataFilePtrPtr):X}";
    }
}