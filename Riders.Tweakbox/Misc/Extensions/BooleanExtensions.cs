using System;
namespace Riders.Tweakbox.Misc.Extensions;

public static class BooleanExtensions
{
    /// <summary>
    /// Executes a specified action if the value is true.
    /// </summary>
    public static void ExecuteIfTrue(this bool value, Action action)
    {
        if (value)
            action();
    }

}
