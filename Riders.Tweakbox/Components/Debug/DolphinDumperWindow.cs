using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DearImguiSharp;
using Reloaded.Memory;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.Sources;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Shell;
using Sewer56.SonicRiders.Structures.Gameplay;
using Constants = Sewer56.Imgui.Misc.Constants;
using Player = Sewer56.SonicRiders.API.Player;

namespace Riders.Tweakbox.Components.Debug
{
    public class DolphinDumperWindow : ComponentBase
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Dolphin Dumper Window";

        /// <inheritdoc />
        public override void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                ImGui.TextWrapped(
                    "This window imports known data (Gear Data etc.) from the US GameCube version of Sonic Riders running on Dolphin Emulator. " +
                    "This is intended to be used for importing data from prior released modifications such as \"Sonic Riders Netplay/Tournament Edition and DX\". " +
                    "In order for this to work, Dolphin should be running and the game should be launched.\n\n" +
                    "Note: Use with ancient x86 versions of Dolphin because I'm lazy to fix my library to work around reading x64 processes from x86.");

                try
                {
                    if (ImGui.Button("Import Gears", Constants.ButtonSize))
                    {
                        ImportGears();
                    }
                }
                catch (Exception e)
                {
                    Shell.AddDialog("Import Operation Failed", $"{e.ToString()} | {e.Message}\n{e.StackTrace}");
                }
            }

            ImGui.End();
        }

        private unsafe void ImportGears()
        {
            if (!TryGetDolphin(out Dolphin.Memory.Access.Dolphin dolphin, out Process process))
                return;

            if (!dolphin.TryGetAddress(0x805E5F40, out var dolphinAddress))
                return;

            var memory = new ExternalMemory(process);
            var pointer = new FixedArrayPtr<ExtremeGear>((ulong) dolphinAddress, Player.NumberOfGears, true, memory);
            var nativeGearPtr = (ExtremeGear*) Player.Gears.Pointer;

            for (var x = 0; x < pointer.Count; x++)
            {
                // Copy gear data.
                var copiedGearPtr = pointer.GetPointerToElement(x);
                memory.ReadRaw((IntPtr) copiedGearPtr, out byte[] copiedGearBytes, pointer.ElementSize);

                // Swap endian of known data.
                Reflection.SwapStructEndianness(typeof(ExtremeGear), copiedGearBytes);

                Struct.FromArray(copiedGearBytes, out ExtremeGear gcGear, true, 0);
                ref var nativeGear = ref Unsafe.AsRef<ExtremeGear>(&nativeGearPtr[x]);

                // Reverse endian all field members.
                Reflection.ShallowCopyValues(ref gcGear, ref nativeGear);
            }
        }

        private bool TryGetDolphin(out Dolphin.Memory.Access.Dolphin dolphin, out Process process)
        {
            process = Process.GetProcessesByName("dolphin")[0];
            dolphin = new Dolphin.Memory.Access.Dolphin(process);
            return dolphin.TryGetBaseAddress(out _);
        }
    }
}
