using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.SonicRiders.Structures.Enums;
using System.Numerics;
using Sewer56.SonicRiders.API;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;
using PlayerAPI = Sewer56.SonicRiders.API.Player;
using Riders.Tweakbox.Interfaces.Structs.Enums;
using Riders.Tweakbox.Misc.Log;

namespace Riders.Tweakbox.Controllers.Modifiers;

/// <summary>
/// Provides a simple implementation of SlipStream.
/// </summary>
public struct SlipstreamModifier
{
    /// <summary>
    /// Contains the slipstream details for the last frame applied.
    /// Used for debugging.
    /// </summary>
    public SlipstreamDebugInfo[,] SlipstreamDebugInformation = new SlipstreamDebugInfo[PlayerAPI.MaxNumberOfPlayers, PlayerAPI.MaxNumberOfPlayers];

    /// <summary>
    /// Stores the total slipstream power applied to a player.
    /// </summary>
    public float[] TotalSlipPower = new float[PlayerAPI.MaxNumberOfPlayers];

    /// <summary>
    /// For debug use.
    /// Always calculates slipstream, even if it will not apply it.
    /// </summary>
    public bool AlwaysCalculateSlipstream = false;

    private ref GameModifiers Modifiers => ref _modifiersController.Modifiers;
    private GameModifiersController _modifiersController;

    public SlipstreamModifier(GameModifiersController gameModifiersController)
    {
        _modifiersController = gameModifiersController;
    }

    internal unsafe int OnAfterRunPhysicsSimulation()
    {
        if (!Modifiers.Slipstream.Enabled)
            return 0;

        var maxPlayers = *State.NumberOfRacers;
        for (int x = 0; x < maxPlayers; x++)
        {
            // Check if player eligible for slipstream.
            ref var player = ref PlayerAPI.Players[x];
            bool applySlipstream = player.PlayerState == PlayerState.NormalOnBoard;
            if (!applySlipstream && !AlwaysCalculateSlipstream)
                continue;

            float slipPower = 0;
            for (int y = 0; y < maxPlayers; y++)
            {
                if (x == y)
                    continue;

                ref var otherPlayer = ref PlayerAPI.Players[y];
                DoSlipstream(ref player, ref otherPlayer, Modifiers.Slipstream, ref SlipstreamDebugInformation[x, y], out float slipAdditive);
                slipPower += slipAdditive;
            }

            // Apply Slipstream
            slipPower += 1;
            TotalSlipPower[x] = slipPower;
            if (applySlipstream && !*State.IsPaused)
                player.Speed *= slipPower;
        }

        return 0;
    }

    private void DoSlipstream(ref Player first, ref Player second, in SlipstreamModifierSettings modifier, ref SlipstreamDebugInfo debugInfo, out float slipPower)
    {
        // NaN guard.
        if (first.Position == second.Position)
        {
            slipPower = 0;
            return;
        }

        // Prepare Variables.
        double slipDistanceMult = default;
        double slipAnglePower = default;
        double slipAlignmentPower = default;

        // Get Slipstream Angle & Distance Bonus
        var secondForwardVector = second.Rotation.GetForwardVectorForRidersRotation();
        var directionToSecond   = Vector3.Normalize(first.Position - second.Position);
        var angle               = Vector3Extensions.CalcAngle(secondForwardVector, directionToSecond);
        if (float.IsNaN(angle)) { slipPower = 0; return; }

        var firstForwardVector = first.Rotation.GetForwardVectorForRidersRotation();
        var alignment          = Vector3Extensions.CalcAngle(firstForwardVector, secondForwardVector);
        if (float.IsNaN(alignment)) { slipPower = 0; return; }

        var distance  = Vector3.Distance(first.Position, second.Position);

        if (angle > modifier.SlipstreamMaxAngle || alignment > modifier.SlipstreamMaxAngle || distance > modifier.SlipstreamMaxDistance)
        {
            slipAnglePower = 0;
            slipDistanceMult = 0;
            slipAlignmentPower = 0;
        }
        else
        {
            slipAlignmentPower = ((1 - (alignment / modifier.SlipstreamMaxAngle)));
            slipAnglePower = ((1 - (angle / modifier.SlipstreamMaxAngle)));
            slipDistanceMult = (1 - (distance / modifier.SlipstreamMaxDistance));
        }

        var combinedStrength = slipAlignmentPower * slipAnglePower * modifier.SlipstreamMaxStrength;
        slipPower = (float)(combinedStrength * modifier.EasingSetting.GetEasingFunction().Ease(slipDistanceMult));

        // Assign debug info.
        debugInfo.SlipPower = slipPower;
        debugInfo.SlipAnglePower = (float)slipAnglePower;
        debugInfo.SlipAlignmentPower = (float)slipAlignmentPower;
        debugInfo.SlipDistanceMult = (float)slipDistanceMult;

        debugInfo.Alignment = alignment;
        debugInfo.Angle = angle;
        debugInfo.Distance = distance;
    }

    /// <summary>
    /// Contains debugging info for slipstream.
    /// </summary>
    public struct SlipstreamDebugInfo
    {
        public float Angle;
        public float Alignment;
        public float SlipPower;

        public float SlipAlignmentPower;
        public float SlipAnglePower;
        public float SlipDistanceMult;
        public float Distance;
    }
}
