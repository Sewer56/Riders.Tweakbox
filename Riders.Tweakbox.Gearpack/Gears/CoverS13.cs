using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class CoverS13 : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "CoverS 1.3";

    private ShortcutBehaviour _shortcutBehaviour = new ShortcutBehaviour()
    {
        FlyShortcutModifier = 1.075f,
        SpeedShortcutModifier = 1.075f,
        PowerShortcutAddedSpeed = 1.075f,
    };

    // IExtremeGear API Callbacks //
    public ShortcutBehaviour GetShortcutBehaviour() => _shortcutBehaviour;
}