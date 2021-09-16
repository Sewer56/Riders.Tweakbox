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
    // Pathfinding
    private IAsmHook _hookD;

    // Gear Description
    private IAsmHook _hookE;

#pragma warning disable IDE0060 // Remove unused parameter
    public unsafe CustomGearPatches(CustomGearCodePatcher codePatcher) // Unused because I want people to know other class needs to be made first.
#pragma warning restore IDE0060 // Remove unused parameter
    {
        var hooks = IoC.GetSingleton<IReloadedHooks>();

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
}
