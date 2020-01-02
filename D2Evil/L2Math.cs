using System;

namespace D2Evil
{
    public class L2Math
    {
        public const double Deg2Rad = Math.PI / 180.0;
        public const double Rad2Deg = 180.0 / Math.PI;
        public const float Deg2RadF = 0.0174532924f;
        public const float Rad2DegF = 57.29578f;

        /// <summary>
        /// Result: [-PI, PI]
        /// </summary>
        /// <returns></returns>
        public static double AngleDiff(double q1, double q2)
        {
            double d = q1 - q2;
            while (d < -Math.PI)
            {
                d += 2 * Math.PI;
            }
            while (d > Math.PI)
            {
                d -= 2 * Math.PI;
            }

            return d;
        }
    }
}
