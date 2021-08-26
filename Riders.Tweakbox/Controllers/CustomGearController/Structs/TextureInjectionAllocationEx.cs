using Riders.Tweakbox.Services.TextureGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Riders.Tweakbox.Controllers.CustomGearController.Structs;


public class TextureInjectionAllocationEx : PvrtTextureInjectionAllocation
{
    /// <summary>
    /// The index of the first texture in this allocation.
    /// </summary>
    public int TextureIndex { get; set; }

    public TextureInjectionAllocationEx(PvrtTextureInjectionAllocation existing) : base(existing) { }
}
