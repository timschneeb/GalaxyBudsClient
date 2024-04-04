using System;
using System.Numerics;

namespace GalaxyBudsClient.Utils.Extensions;

public static class MathExtensions
{
    public static float[] ToRollPitchYaw(this Quaternion q)
    {
        var result = new float[3];
            
        result[0] /* roll */  = (float) Math.Atan2(2.0 * (q.Z * q.Y + q.W * q.X) , 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y));
        result[1] /* pitch */ = (float) Math.Asin(2.0 * (q.Y * q.W - q.Z * q.X));
        result[2] /* yaw */ = (float) Math.Atan2(2.0 * (q.Z * q.W + q.X * q.Y) , - 1.0 + 2.0 * (q.W * q.W + q.X * q.X));

        return result;
    }
}