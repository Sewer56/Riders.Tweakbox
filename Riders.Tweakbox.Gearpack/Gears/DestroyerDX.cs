using System;
using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Gearpack.Gears;

public class DestroyerDX : CustomGearBase, IExtremeGear
{
    public override string FolderName { get; set; } = "Destroyer DX";

    private DashPanelGearProperties _dashPanelProps;
    private MonoTypeShortcutBehaviourDX _monoTypeShortcutBehaviour;

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void InitializeGear(string gearsFolder, ICustomGearApi gearApi)
    {
        _dashPanelProps = new DashPanelGearProperties()
        {
            SetSpeedGain = SetSpeedGainFromDashPanel
        };

        _monoTypeShortcutBehaviour = new MonoTypeShortcutBehaviourDX()
        {
            ExistingTypeSpeedModifierPercent = 1.15f,
            NewTypeSpeedModifierPercent = 0.95f
        };
    }

    // IExtremeGear API Callbacks //
    public MonoTypeShortcutBehaviourDX GetMonoTypeShortcutBehaviour() => _monoTypeShortcutBehaviour;

    public DashPanelGearProperties GetDashPanelProperties() => _dashPanelProps;

    private unsafe float SetSpeedGainFromDashPanel(IntPtr playerPtr, int playerIndex, int playerLevel, float speed)
    {
        var player = (Player*)playerPtr;
        if (IsMonoShortcut(player))
            speed += Utility.SpeedometerToFloat(10);

        return speed;
    }

    private unsafe bool IsMonoShortcut(Player* player)
    {
        ref var characterParameter = ref Sewer56.SonicRiders.API.Player.CharacterParameters[(int)player->Character];
        return player->ExtremeGearPtr->ExtraTypes.ContainsType((FormationTypes)characterParameter.ShortcutType);
    }
}
