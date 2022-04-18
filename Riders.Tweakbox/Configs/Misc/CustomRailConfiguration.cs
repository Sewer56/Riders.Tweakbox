using System;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
using Riders.Tweakbox.Configs.Json;

namespace Riders.Tweakbox.Configs.Misc;

public class CustomRailConfiguration : JsonConfigBase<CustomRailConfiguration, CustomRailConfiguration.Internal>
{
    public class Internal
    {
        /// <summary>
        /// The list of rails contained in this configuration.
        /// </summary>
        public List<RailEntry> Rails = new();

        public Internal() { }

        public Internal(int numRails)
        {
            Rails = new List<RailEntry>(numRails);
            for (int x = 0; x < numRails; x++)
                Rails.Add(new RailEntry());
        }
    }

    public class RailEntry
    {
        /// <summary>
        /// True if this rail's behaviour should be modified.
        /// </summary>
        public bool IsEnabled = false;

        /// <summary>
        /// Number of frames between initial speed cap and final speed cap.
        /// </summary>
        public int Frames;
        
        /// <summary>
        /// Setting used to interpolate between initial and final speed.
        /// </summary>
        public EasingSetting EasingSetting;

        /// <summary>
        /// Whether to Ease In/Out/Half
        /// </summary>
        public EasingMode EasingMode = EasingMode.EaseIn;

        /// <summary>
        /// Initial speed cap to assign to the rail.
        /// </summary>
        public float SpeedCapInitial;
        
        /// <summary>
        /// Final speed cap to assign to the rail.
        /// </summary>
        public float SpeedCapEnd;

        /// <summary>
        /// Calculates the expected player speed on the rail.
        /// </summary>
        /// <param name="framesOnRail">The amount of frames player spent on rail.</param>
        public float CalculateSpeed(int framesOnRail)
        {
            if (Frames == 0)
                return SpeedCapInitial;
            
            var easingFunction = EasingSetting.GetEasingFunction();
            easingFunction.EasingMode = EasingMode;
            var timeFactor = Math.Min(1, ((float) framesOnRail) / Frames);
            var speedDiff = SpeedCapEnd - SpeedCapInitial;
            var speedAboveInitial = speedDiff * easingFunction.Ease(timeFactor);
            var speed = (float)(SpeedCapInitial + speedAboveInitial);
            return speed;
        }
    }
}