using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Internal;
using Riders.Tweakbox.Interfaces.Structs;
using System;

namespace Riders.Tweakbox.Api.Children;

public class PhysicsApi : IPhysicsApi
{
    /// <inheritdoc />
    public ref DashPanelProperties GetDashPanelPropertiesRef() => ref Static.PanelProperties;

    /// <inheritdoc />
    public ref SpeedShoeProperties GetSpeedShoePropertiesRef() => ref Static.SpeedShoeProperties;

    /// <inheritdoc />
    public ref DecelProperties GetDecelPropertiesRef() => ref Static.DecelProperties;
}
