using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Riders.Netplay.Messages.Unreliable;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Netplay.Messages.Tests
{
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
            result.Air = (uint?)random.Next(0, 200000);
            result.Rings = (byte?)random.Next(0, 99);
            result.LastState = (byte?)random.Next(0, 32);
            result.State = (byte?)random.Next(0, 32);
            result.Velocity = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
            result.TurningAmount = (float?)random.NextDouble();
            result.LeanAmount = (float?)random.NextDouble();
            result.ControlFlags = (PlayerControlFlags?)random.Next(0, 0xFFFFFF);
            result.Animation = (byte?)random.Next(0, 99);
            result.LastAnimation = (byte?)random.Next(0, 99);
            result.LapCounter = (byte?)random.Next(0, 99);

            return result;
        }
    }
}
