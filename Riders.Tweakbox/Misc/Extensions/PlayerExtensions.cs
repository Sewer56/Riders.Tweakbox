using System.Numerics;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Misc.Extensions;

public static class PlayerExtensions
{
    /// <summary>
    /// Teleports the player to a given position and rotation; without preserving state or speed.
    /// </summary>
    /// <param name="player">The player to teleport.</param>
    /// <param name="position">Position of the player.</param>
    /// <param name="rotation">Rotation of the player.</param>
    public static void Teleport(this ref Player player, Vector3 position, Vector3 rotation)
    {
        player.Position = position;
        player.PositionAlt = position;
        player.Rotation = rotation;
        player.Speed = 0;
        player.VSpeed = 0;
        player.Acceleration = 0;
        player.FallingMode = FallingMode.Ground;
        player.LastMovementFlags = 0;
        player.MovementFlags = 0;
    }
}