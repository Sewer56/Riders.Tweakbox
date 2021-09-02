using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using ExtremeGear = Sewer56.SonicRiders.Structures.Gameplay.ExtremeGear;
using ExtremeGearIndex = Sewer56.SonicRiders.Structures.Enums.ExtremeGear;

namespace Riders.Tweakbox.Controllers.CustomGearController;

/// <summary>
/// A workaround for exhaust trail issues related to accessing data at offset based on gear index.
/// </summary>
internal unsafe class CustomGearPatches
{
    // Exhaust Trails
    private IAsmHook _hookA;
    private IAsmHook _hookB;
    private IAsmHook _hookC;

    // Pathfinding
    private IAsmHook _hookD;

    // Gear Description
    private IAsmHook _hookE;

    private byte* ModelToIdMapPtr;
    private byte[] ModelToIdMap;

#pragma warning disable IDE0060 // Remove unused parameter
    public unsafe CustomGearPatches(CustomGearCodePatcher codePatcher) // Unused because I want people to know other class needs to be made first.
#pragma warning restore IDE0060 // Remove unused parameter
    {
        var hooks = IoC.GetSingleton<IReloadedHooks>();

        // Populate Map of Gears to Models
        ModelToIdMap = GC.AllocateArray<byte>(CustomGearCodePatcher.MaxGearCount + 1, true);
        var gearPtr = (ExtremeGear*) Player.Gears.GetPointerToElement(0);
        for (int x = 0; x < codePatcher.OriginalGearCount; x++)
        {
            ModelToIdMap[(int)gearPtr->GearModel] = (byte)x;
            gearPtr++;
        }

        // This is safe because ModelToIdMap is on the Pinned Object Heap (POH)
        fixed (byte* modelToIdMapPtr = ModelToIdMap)
            ModelToIdMapPtr = modelToIdMapPtr;

        // Exhaust Trails 
        _hookA = hooks.CreateAsmHook(GetAsmSetByModel("esi", "eax", "ecx"), 0x4134FC).Activate();
        _hookB = hooks.CreateAsmHook(GetAsmSetByModel("eax", "esi", "ecx"), 0x413E89).Activate();
        _hookC = hooks.CreateAsmHook(GetAsmSetByModel("eax", "esi", "ecx"), 0x413F2D).Activate();

        // AI Pathing
        _hookD = hooks.CreateAsmHook(GetAsmSetDefault("eax"), 0x4B9158).Activate();

        // Patch Gear Description
        _hookE = hooks.CreateAsmHook(GetAsmSetDefault("edx"), 0x461987).Activate();
    }

    private string[] GetAsmSetDefault(string registerToEdit)
    {
        return new string[]
        {
            "use32",
            $"cmp {registerToEdit}, {Player.OriginalNumberOfGears}", // Original
            "jle exit",
            $"mov {registerToEdit}, 0",
            "exit:"
        };
    }

    private unsafe string[] GetAsmSetByModel(string returnReg, string r2, string r3)
    {
        return new string[]
        {
            "use32",
            $"cmp {returnReg}, {Player.OriginalNumberOfGears}", // Original
            "jle exit",

            // Save Register
            $"push {r2}",
            $"push {r3}",

            // Get Board Data
            $"mov {r3}, {returnReg}",
            $"imul {r3}, {sizeof(ExtremeGear)}",
            $"add {r3}, {(long)Player.Gears.Pointer}", // Gear struct now in r3.

            // Check Model to Id Map
            $"movzx {r2}, byte [{r3} + 0x05]",      // Gear model
            $"add {r2}, {(long)ModelToIdMapPtr}", 
            $"movzx {r2}, byte [{r2}]",             // Obtained id from map.
            $"cmp {r2}, {0}",                       // If not in map (or default); back out to fallback.
            $"je assignByGearType",

            $"mov {returnReg}, {r2}",               // Obtain from Map Success!
            $"jmp restoreAndExit",

            // Otherwise assign trail by gear type.
            $"assignByGearType:",
            $"movzx {r2}, byte [{r2} + 0x04]", // Gear Type

            // Give Board Gear Ids Board Trails
            $"cmp {r2}, {(int)GearType.Bike}",
            $"jne checkSkate",
            $"mov {returnReg}, {(int)ExtremeGearIndex.ERider}",
            $"jmp restoreAndExit",

            $"checkSkate:",
            $"cmp {r2}, {(int)GearType.Skate}",
            $"jne default",
            $"mov {returnReg}, {(int)ExtremeGearIndex.Darkness}",
            $"jmp restoreAndExit",

            $"default:", // And Default Case
            $"mov {returnReg}, {(int)ExtremeGearIndex.Default}",

            // Restore Register
            "restoreAndExit:",
            $"pop {r3}",
            $"pop {r2}",

            "exit:"
        };
    }
}
