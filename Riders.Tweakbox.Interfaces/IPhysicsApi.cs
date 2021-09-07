using Riders.Tweakbox.Interfaces.Structs;

namespace Riders.Tweakbox.Interfaces;

public interface IPhysicsApi
{
    /// <summary>
    /// Retrieves the current current Dash Panel settings as a reference.
    /// </summary>
    public ref DashPanelProperties GetDashPanelPropertiesRef();

    /// <summary>
    /// Retrieves the current Speed Shoe settings as a reference.
    /// </summary>
    public ref SpeedShoeProperties GetSpeedShoePropertiesRef();

    /// <summary>
    /// Retrieves the current deceleration settings as a reference.
    /// </summary>
    public ref DecelProperties GetDecelPropertiesRef();
}
