using Riders.Tweakbox.Services.TextureGen;

namespace Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;


public class TextureInjectionAllocationEx : PvrtTextureInjectionAllocation
{
    /// <summary>
    /// The index of the first texture in this allocation.
    /// </summary>
    public int TextureIndex { get; set; }

    public TextureInjectionAllocationEx(PvrtTextureInjectionAllocation existing) : base(existing) { }
}
