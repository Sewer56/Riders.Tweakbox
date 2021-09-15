using System;
using System.Numerics;
using Riders.Netplay.Messages.Unreliable;
using Riders.Netplay.Messages.Unreliable.Enums;
using Riders.Netplay.Messages.Unreliable.Structs;
namespace Riders.Netplay.Messages.Tests;

public class Utilities
{
    /// <summary>
    /// Gets a random instance of a player.
    /// </summary>
    public static UnreliablePacketPlayer GetRandomPlayer()
    {
        var random = new Random();
        var result = new UnreliablePacketPlayer();

        result.Position = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
        result.RotationX = (float)random.NextDouble();
        result.RotationY = (float)random.NextDouble();
        result.RotationZ = (float)random.NextDouble();
        result.Air = (uint?)random.Next(0, 200000);
        result.Rings = (byte?)random.Next(0, 99);
        result.LastState = (byte?)random.Next(0, 32);
        result.State = (byte?)random.Next(0, 32);
        result.Velocity = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
        result.TurningAmount = (float?)random.NextDouble();
        result.LeanAmount = (float?)random.NextDouble();
        result.ControlFlags = (MinControlFlags?)random.Next(0, (int)MinControlFlags.NoHoverAndTrail);
        result.Animation = (byte?)random.Next(0, 99);
        result.LastAnimation = (byte?)random.Next(0, 99);
        result.AnalogXY = new AnalogXY()
        {
            X = (sbyte)random.Next(-100, 100),
            Y = (sbyte)random.Next(-100, 100)
        };
        result.MovementFlags = new MovementFlags((Reliable.Structs.Gameplay.Shared.MovementFlags)random.Next(0, (int)Reliable.Structs.Gameplay.Shared.MovementFlags.AttachToRail));

        return result;
    }
}
