using System;
using System.Numerics;

namespace GalaxyBudsClient.Utils.Extensions
{
    public static class MathExtensions
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        
        public static Vector3 ToEulerXYZ(this Quaternion q)
        {
            Vector3 euler;

            // if the input quaternion is normalized, this is exactly one. Otherwise, this acts as a correction factor for the quaternion's not-normalizedness
            var unit = (q.X * q.X) + (q.Y * q.Y) + (q.Z * q.Z) + (q.W * q.W);

            // this will have a magnitude of 0.5 or greater if and only if this is a singularity case
            var test = q.X * q.W - q.Y * q.Z;

            if (test > 0.4995f * unit) // singularity at north pole
            {
                euler.X = (float)Math.PI / 2;
                euler.Y = 2f * (float)Math.Atan2(q.Y, q.X);
                euler.Z = 0;
            }
            else if (test < -0.4995f * unit) // singularity at south pole
            {
                euler.X = (float)-Math.PI / 2;
                euler.Y = -2f * (float)Math.Atan2(q.Y, q.X);
                euler.Z = 0;
            }
            else // no singularity - this is the majority of cases
            {
                euler.X = (float)Math.Asin(2f * (q.W * q.X - q.Y * q.Z));
                euler.Y = (float)Math.Atan2(2f * q.W * q.Y + 2f * q.Z * q.X, 1 - 2f * (q.X * q.X + q.Y * q.Y));
                euler.Z = (float)Math.Atan2(2f * q.W * q.Z + 2f * q.X * q.Y, 1 - 2f * (q.Z * q.Z + q.X * q.X));
            }

            // all the math so far has been done in radians. Before returning, we convert to degrees...
            euler.X *= 57.29578049f; // Rad2Deg

            //...and then ensure the degree values are between 0 and 360
            euler.X %= 360;
            euler.Y %= 360;
            euler.Z %= 360;

            return euler;
        }

        public static float[] ToRollPitchYaw(this Quaternion q)
        {
            float[] result = new float[3];
            
            result[0] /* roll */  = (float) Math.Atan2(2.0 * (q.Z * q.Y + q.W * q.X) , 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y));
            result[1] /* pitch */ = (float) Math.Asin(2.0 * (q.Y * q.W - q.Z * q.X));
            result[2] /* yaw */ = (float) Math.Atan2(2.0 * (q.Z * q.W + q.X * q.Y) , - 1.0 + 2.0 * (q.W * q.W + q.X * q.X));

            return result;
        }
    }
}