namespace Riders.Tweakbox.Interfaces.Structs.Characters;

public struct ApiCharacterParameters
{
    /// <summary>
    /// The character stats used for determining what shortcut the character can take.
    /// </summary>
    public CharacterType? ShortcutType;

    /// <summary>
    /// The character stats used for determining what stats the character should use.
    /// </summary>
    public CharacterType? StatsType;

    /// <summary>
    /// The character's gender.
    /// </summary>
    public Gender? Gender;

    /// <summary>
    /// The character's height (affects camera).
    /// </summary>
    public float? Height;

    /// <summary>
    /// The character's speed multiplier (inversely affects accel).
    /// This value is relative to 0, so for 98% use -0.02.
    /// </summary>
    public float? SpeedMultiplierOffset;

    public byte? StatDashLv1;
    public byte? StatDashLv2;
    public byte? StatDashLv3;

    public byte? StatLimitLv1;
    public byte? StatLimitLv2;
    public byte? StatLimitLv3;

    public byte? StatPowerLv1;
    public byte? StatPowerLv2;
    public byte? StatPowerLv3;

    public byte? StatCorneringLv1;
    public byte? StatCorneringLv2;
    public byte? StatCorneringLv3;
}

public enum Gender : int
{
    Male,
    Female
}
public enum CharacterType : byte
{
    Speed,
    Fly,
    Power
}