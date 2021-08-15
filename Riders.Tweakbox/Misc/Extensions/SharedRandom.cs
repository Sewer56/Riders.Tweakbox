namespace Riders.Tweakbox.Misc.Extensions;

/// <summary>
/// Provides access to a static random number generator.
/// </summary>
public static class SharedRandom
{
    public static System.Random Instance { get; private set; } = new System.Random();
}
