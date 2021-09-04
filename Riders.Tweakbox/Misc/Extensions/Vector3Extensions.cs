using System;
using System.Diagnostics;
using System.Numerics;

namespace Riders.Tweakbox.Misc.Extensions;

public static class Vector3Extensions
{
    /// <summary>
    /// Calculates the angle between two vectors.
    /// </summary>
    /// <param name="forwardVectorFirst">The first vector.</param>
    /// <param name="forwardVectorSecond">The second vector.</param>
    /// <returns>The angle in degrees.</returns>
    public static float CalcAngle(this in Vector3 forwardVectorFirst, in Vector3 forwardVectorSecond)
    {
        var dotProduct = Vector3.Dot(forwardVectorFirst, forwardVectorSecond);
        var magnitudes = forwardVectorFirst.Length() * forwardVectorSecond.Length();

        var cosTheta = dotProduct / (magnitudes);
        var arcCos   = Math.Acos(Math.Clamp(cosTheta, -1, 1));
        Debug.Assert(!double.IsNaN(arcCos));

        return (float)(arcCos / Math.PI * 180.0);
    }
    
    /// Returns a forward vector given a rotation.
    /// </summary>
    /// <param name="rotation">Rotation with the X,Y,Z components as radians.</param>
    public static Vector3 GetForwardVector(this in Vector3 rotation)
    {
        var result = Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
        var forwardVector = Vector3.Transform(new Vector3(0, 0, -1), result); // 0, 0, -1 is forward in right hand system.
        return Vector3.Normalize(forwardVector);
    }
}
