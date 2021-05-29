using System;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// Controller that contains minor patches to game code.
    /// </summary>
    public class MiscPatchController : IController
    {
        /// <summary>
        /// Disables overwriting of race position after the race has completed.
        /// </summary>
        public Patch DisableRacePositionOverwrite = new Patch(0x4B40E6, new byte[] { 0xEB, 0x44 });

        /// <summary>
        /// Allows the player to always start a race in character select.
        /// </summary>
        public Patch AlwaysCanStartRaceInCharacterSelect = new Patch(0x004634B8, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, });
    }
}
