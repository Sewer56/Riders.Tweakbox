using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Interop;
using Riders.Tweakbox.Services.Interfaces;
namespace Riders.Tweakbox.Services;

/// <summary>
/// A service that keeps track of the target menu the game intends to return to after leaving a race.
/// </summary>
public unsafe class MenuReturnTargetService : ISingletonService
{
    /// <summary>
    /// Contains a pointer to the current return menu.
    /// Write to this value in your codes as necessary.
    /// </summary>
    public Pinnable<int> Current { get; private set; } = new Pinnable<int>(22);

    /// <summary>
    /// Contains a pointer to the last return menu set by the game.
    /// You should not modify this value, this is for reference only.
    /// </summary>
    public Pinnable<int> Copy { get; private set; } = new Pinnable<int>(22);

    private IAsmHook _getReturnMenuHook;

    public MenuReturnTargetService(IReloadedHooks hooks, IReloadedHooksUtilities utils)
    {
        // Setup Return to Course Select
        var getReturnMenu = new string[]
        {
            "use32",
            $"mov dword [{(long)Current.Pointer}], eax",  // Set Menu to Return To
            $"mov dword [{(long)Copy.Pointer}], eax",  // Set Menu to Return To
        };

        _getReturnMenuHook = hooks.CreateAsmHook(getReturnMenu, 0x0046AC73).Activate();
    }
}
