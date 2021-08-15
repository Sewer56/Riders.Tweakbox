using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
namespace Riders.Tweakbox.Controllers;

/// <summary>
/// Controller that contains minor patches to game code.
/// </summary>
public class MiscPatchController : IController
{
    /// <summary>
    /// Disables overwriting of race position after the race has completed.
    /// </summary>
    public Patch DisableRacePositionOverwrite = new Patch(0x4B40E6, new byte[] { 0xEB, 0x44 });

    /// <summary>
    /// Allows the player to always start a race in character select.
    /// </summary>
    public Patch AlwaysCanStartRaceInCharacterSelect = new Patch(0x004634B8, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, });

    /// <summary>
    /// Allows the player to go backwards.
    /// </summary>
    public Patch EnableGoingBackwards = new Patch(0x4BC751, new byte[] { 0xEB, 0x48 });

    /// <summary>
    /// Skips a 10 frame loading render loop during game state reset used to smoothen the presentation.
    /// </summary>
    public Patch SkipLoadingRenderLoop = new Patch(0x00417019, new byte[] { 0xE9, 0x8B, 0x00, 0x00, 0x00 });

    /// <summary>
    /// Increases the sound effect buffer used to load sound effect files, which improves load times.
    /// </summary>
    public PatchCollection LargerSoundEffectBuffer = new PatchCollection(new Patch[]
    {
            // Increase to 0x00110000
            new Patch(0x004ED534, new byte[] { 0x00, 0x00, 0x11, 0x00 }),
            new Patch(0x00595BE0, new byte[] { 0x00, 0x00, 0x11, 0x00 }),
    });

    /// <summary>
    /// Disables some sort of file info collection/cache made by CRI.
    /// This is separate from their PC File Table patched by the CriFsHook mod.
    /// </summary>
    public Patch DisableCriCacheInit = new Patch(0x00420E52, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 });

    public MiscPatchController()
    {
        SkipLoadingRenderLoop.Enable();
        LargerSoundEffectBuffer.Enable();
        DisableCriCacheInit.Enable();
    }
}
