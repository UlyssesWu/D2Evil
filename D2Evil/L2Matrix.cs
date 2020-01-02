// ReSharper disable InconsistentNaming

namespace D2Evil
{
    internal class L2Matrix2x3F
    {
        private float m00 = 1f;

        private float m10;

        private float m01;

        private float m11 = 1f;

        private float m02;

        private float m12;

        internal L2Matrix2x3F(float m00, float m10, float m01, float m11, float m02, float m12)
        {
            this.m00 = m00;
            this.m10 = m10;
            this.m01 = m01;
            this.m11 = m11;
            this.m02 = m02;
            this.m12 = m12;
            //Some unknown transform
        }

        internal L2Matrix2x3F(double m00, double m10, double m01, double m11, double m02, double m12)
        {
            this.m00 = (float)m00;
            this.m10 = (float)m10;
            this.m01 = (float)m01;
            this.m11 = (float)m11;
            this.m02 = (float)m02;
            this.m12 = (float)m12;
            //Some unknown transform
        }
    }
}
