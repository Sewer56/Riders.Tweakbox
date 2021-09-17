﻿using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class GrinderDX : CustomGearBase, IExtremeGear
{
    private MonoTypeShortcutBehaviourDX _monoTypeShortcutBehaviour = new MonoTypeShortcutBehaviourDX()
    {
        ExistingTypeSpeedModifierPercent = 1.125f,
        NewTypeSpeedModifierPercent = 0.95f
    };

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "Grinder DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    // IExtremeGear API Callbacks //
    public MonoTypeShortcutBehaviourDX GetMonoTypeShortcutBehaviour() => _monoTypeShortcutBehaviour;
}