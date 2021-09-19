using System;

namespace Riders.Tweakbox.Interfaces.Structs;

/// <summary>
/// Provides replacement pointers to various API endpoints in Sewer56.SonicRiders.
/// </summary>
public class ApiPointers
{
    /// <summary>
    /// New pointer to the gears used in <see cref="Sewer56.SonicRiders.API.Players.Gears"/>.
    /// </summary>
    public ApiPointer Gears;
}

public struct ApiPointer
{
    public IntPtr Address;
    public int NumItems;

    public ApiPointer(IntPtr address, int numItems)
    {
        Address = address;
        NumItems = numItems;
    }
}