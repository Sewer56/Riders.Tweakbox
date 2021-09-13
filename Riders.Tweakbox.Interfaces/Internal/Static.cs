using System;
using System.ComponentModel;
using Reloaded.Memory.Interop;
using Riders.Tweakbox.Interfaces.Structs;
namespace Riders.Tweakbox.Interfaces.Internal;

/// <summary>
/// INTERNAL API. DO NOT USE.
/// Use <see cref="IPhysicsApi"/> in <see cref="ITweakboxApiImpl"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class Static
{
    /// <summary>
    /// The current Dash Panel settings.
    /// </summary>
    public static DashPanelProperties PanelProperties = DashPanelProperties.Default();

    /// <summary>
    /// The current Speed Shoe settings.
    /// </summary>
    public static SpeedShoeProperties SpeedShoeProperties = SpeedShoeProperties.Default();

    /// <summary>
    /// The current acceleration settings.
    /// </summary>
    public static Pinnable<DecelProperties> DecelProperties = new Pinnable<DecelProperties>(Structs.DecelProperties.GetDefault());
}
