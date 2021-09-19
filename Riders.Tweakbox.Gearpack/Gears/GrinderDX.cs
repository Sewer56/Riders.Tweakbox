using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Gearpack.Gears;

public class GrinderDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Grinder DX";

    private MonoTypeShortcutBehaviourDX _monoTypeShortcutBehaviour = new MonoTypeShortcutBehaviourDX()
    {
        ExistingTypeSpeedModifierPercent = 1.125f,
        NewTypeSpeedModifierPercent = 0.95f
    };

    // IExtremeGear API Callbacks //
    public MonoTypeShortcutBehaviourDX GetMonoTypeShortcutBehaviour() => _monoTypeShortcutBehaviour;
}
