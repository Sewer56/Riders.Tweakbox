namespace Riders.Tweakbox.Controllers.CustomGearController.Structs;

/// <summary>
/// Contains the result of adding gear data.
/// </summary>
public unsafe class AddGearDataResult
{
    /// <summary>
    /// Index of the gear added.
    /// Note that this index can change e.g. a user enters netplay.
    /// If you need to have an up to date index, please add an event handler to
    /// <see cref="AddGearData.OnIndexChanged"/>.
    /// </summary>
    public int GearIndex;
}
