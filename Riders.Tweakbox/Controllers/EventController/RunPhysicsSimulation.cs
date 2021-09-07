using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Utility;
using static Sewer56.SonicRiders.Functions.Functions;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Executed before the code to run 1 frame of physics simulation.
    /// </summary>
    public static event Functions.SaveAllRegistersReturnIntFn OnRunPhysicsSimulation;

    /// <summary>
    /// Executed after the code to run 1 frame of physics simulation.
    /// </summary>
    public static event Functions.SaveAllRegistersReturnIntFn AfterRunPhysicsSimulation;

    /// <summary>
    /// Replaces the code to run 1 frame of physics simulation.
    /// </summary>
    public static event SaveAllRegistersReturnIntFnHandler RunPhysicsSimulation;

    private static IHook<Functions.SaveAllRegistersReturnIntFnPtr> _runPhysicsSimulationHook;

    public void InitRunPhysicsSimulation(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _runPhysicsSimulationHook = Functions.RunPhysicsSimulation.HookAs<SaveAllRegistersReturnIntFnPtr>(typeof(EventController), nameof(RunPhysicsSimulationHook)).Activate();
    }

    [UnmanagedCallersOnly]
    private static int RunPhysicsSimulationHook()
    {
        OnRunPhysicsSimulation?.Invoke();
        var result = RunPhysicsSimulation?.Invoke(_runPhysicsSimulationHook) ?? _runPhysicsSimulationHook.OriginalFunction.Value.Invoke();
        AfterRunPhysicsSimulation?.Invoke();
        return result;
    }

    /// <summary/>
    public delegate int SaveAllRegistersReturnIntFnHandler(IHook<Functions.SaveAllRegistersReturnIntFnPtr> hook);
}