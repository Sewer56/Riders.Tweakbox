using System.Collections.Generic;
using Sewer56.SonicRiders.Structures.Enums;
namespace Riders.Tweakbox.Services.RichPresence.Common;

public class ImageNameDictionary
{
    public static Dictionary<Levels, string> Images = new Dictionary<Levels, string>()
    {
        { Levels.MetalCity, nameof(Levels.MetalCity).ToLower() },
        { Levels.SplashCanyon, nameof(Levels.SplashCanyon).ToLower() },
        { Levels.EggFactory, nameof(Levels.EggFactory).ToLower() },
        { Levels.GreenCave, nameof(Levels.GreenCave).ToLower() },
        { Levels.SandRuins, nameof(Levels.SandRuins).ToLower() },
        { Levels.BabylonGarden, nameof(Levels.BabylonGarden).ToLower() },
        { Levels.DigitalDimension, nameof(Levels.DigitalDimension).ToLower() },
        { Levels.SEGACarnival, nameof(Levels.SEGACarnival).ToLower() },

        { Levels.NightChase, nameof(Levels.NightChase).ToLower() },
        { Levels.RedCanyon, nameof(Levels.RedCanyon).ToLower() },
        { Levels.IceFactory, nameof(Levels.IceFactory).ToLower() },
        { Levels.WhiteCave, nameof(Levels.WhiteCave).ToLower() },
        { Levels.DarkDesert, nameof(Levels.DarkDesert).ToLower() },
        { Levels.SkyRoad, nameof(Levels.SkyRoad).ToLower() },
        { Levels.BabylonGuardian, nameof(Levels.BabylonGuardian).ToLower() },
        { Levels.SEGAIllusion, nameof(Levels.SEGAIllusion).ToLower() },

        { Levels.DualTowers, nameof(Levels.DualTowers).ToLower() },
        { Levels.SnowValley, nameof(Levels.SnowValley).ToLower() },
        { Levels.SpaceTheater, nameof(Levels.SpaceTheater).ToLower() },
    };
}
