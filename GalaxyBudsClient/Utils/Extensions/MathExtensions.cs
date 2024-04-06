using System;
using System.Numerics;

namespace GalaxyBudsClient.Utils.Extensions;

public static class MathExtensions
{
    public static (float roll, float pitch, float yaw) ToRollPitchYaw(this Quaternion q)
    {
        var roll  = (float) Math.Atan2(2.0 * (q.Z * q.Y + q.W * q.X) , 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y));
        var pitch = (float) Math.Asin(2.0 * (q.Y * q.W - q.Z * q.X));
        var yaw = (float) Math.Atan2(2.0 * (q.Z * q.W + q.X * q.Y) , - 1.0 + 2.0 * (q.W * q.W + q.X * q.X));
        return (roll, pitch, yaw);
    }
}