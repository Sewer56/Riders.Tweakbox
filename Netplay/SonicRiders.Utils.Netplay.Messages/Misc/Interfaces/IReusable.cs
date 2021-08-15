namespace Riders.Netplay.Messages.Misc.Interfaces;

/// <summary>
/// Represents a type that can be reused in memory.
/// </summary>
public interface IReusable
{
    /// <summary>
    /// Resets all fields of this type.
    /// </summary>
    public void Reset();
}
