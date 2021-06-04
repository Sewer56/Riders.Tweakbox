using System;
using System.Numerics;
using Sewer56.SonicRiders.Utility.Math;

namespace Riders.Tweakbox.Misc.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 DegreesToRadians(this Vector3 current) => new Vector3(DegreesToRadians(current.X), DegreesToRadians(current.Y), DegreesToRadians(current.Z));
        public static Vector3 RadiansToDegrees(this Vector3 current) => new Vector3(RadiansToDegrees(current.X), RadiansToDegrees(current.Y), RadiansToDegrees(current.Z));

        public static Vector3 DegreesToBams(this Vector3 current) => new Vector3(DegreesToBams(current.X), DegreesToBams(current.Y), DegreesToBams(current.Z));
        public static Vector3Int DegreesToBamsInt(this Vector3 current) => new Vector3Int((int) DegreesToBams(current.X), (int) DegreesToBams(current.Y), (int) DegreesToBams(current.Z));
        
        public static float RadiansToDegrees(this float value) => (float) (value / Math.PI * 180.0);
        public static float DegreesToRadians(this float value) => (float) (value / 180.0 * Math.PI);
        public static float DegreesToBams(this float value) => (float) (value * 182.04443);
    }
}
