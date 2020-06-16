//This is a port of java.awt.geom.AffineTransform
using System;
// ReSharper disable InconsistentNaming

namespace FreeLive
{
    public class AffineTransform : ICloneable
    {
        public const int TYPE_IDENTITY = 0;
        public const int TYPE_TRANSLATION = 1;
        public const int TYPE_UNIFORM_SCALE = 2;
        public const int TYPE_GENERAL_SCALE = 4;
        public const int TYPE_QUADRANT_ROTATION = 8;
        public const int TYPE_GENERAL_ROTATION = 16;
        public const int TYPE_GENERAL_TRANSFORM = 32;
        public const int TYPE_FLIP = 64;
        public const int TYPE_MASK_SCALE = TYPE_UNIFORM_SCALE | TYPE_GENERAL_SCALE;
        public const int TYPE_MASK_ROTATION = TYPE_QUADRANT_ROTATION | TYPE_GENERAL_ROTATION;

        /**
         * The <code>TYPE_UNKNOWN</code> is an initial type value
         */
        const int TYPE_UNKNOWN = -1;

        /**
         * The min value equivalent to zero. If absolute value less then ZERO it considered as zero.  
         */
        const double ZERO = 1E-10;

        /**
         * The values of transformation matrix
         */
        private double _m00;
        private double _m10;
        private double _m01;
        private double _m11;
        private double _m02;
        private double _m12;

        /**
         * The transformation <code>type</code> 
         */
        private int _type;

        public AffineTransform()
        {
            _type = TYPE_IDENTITY;
            _m00 = _m11 = 1.0;
            _m10 = _m01 = _m02 = _m12 = 0.0;
        }

        public AffineTransform(AffineTransform t)
        {
            _type = t._type;
            _m00 = t._m00;
            _m10 = t._m10;
            _m01 = t._m01;
            _m11 = t._m11;
            _m02 = t._m02;
            _m12 = t._m12;
        }

        public AffineTransform(float m00, float m10, float m01, float m11, float m02, float m12)
        {
            _type = TYPE_UNKNOWN;
            this._m00 = m00;
            this._m10 = m10;
            this._m01 = m01;
            this._m11 = m11;
            this._m02 = m02;
            this._m12 = m12;
        }

        public AffineTransform(double m00, double m10, double m01, double m11, double m02, double m12)
        {
            _type = TYPE_UNKNOWN;
            this._m00 = m00;
            this._m10 = m10;
            this._m01 = m01;
            this._m11 = m11;
            this._m02 = m02;
            this._m12 = m12;
        }

        public AffineTransform(float[] matrix)
        {
            _type = TYPE_UNKNOWN;
            _m00 = matrix[0];
            _m10 = matrix[1];
            _m01 = matrix[2];
            _m11 = matrix[3];
            if (matrix.Length > 4)
            {
                _m02 = matrix[4];
                _m12 = matrix[5];
            }
        }

        public AffineTransform(double[] matrix)
        {
            _type = TYPE_UNKNOWN;
            _m00 = matrix[0];
            _m10 = matrix[1];
            _m01 = matrix[2];
            _m11 = matrix[3];
            if (matrix.Length > 4)
            {
                _m02 = matrix[4];
                _m12 = matrix[5];
            }
        }

        /*
         * Method returns type of affine transformation.
         * 
         * Transform matrix is
         *   m00 m01 m02
         *   m10 m11 m12
         * 
         * According analytic geometry new basis vectors are (m00, m01) and (m10, m11), 
         * translation vector is (m02, m12). Original basis vectors are (1, 0) and (0, 1). 
         * Type transformations classification:  
         *   TYPE_IDENTITY - new basis equals original one and zero translation
         *   TYPE_TRANSLATION - translation vector isn't zero  
         *   TYPE_UNIFORM_SCALE - vectors length of new basis equals
         *   TYPE_GENERAL_SCALE - vectors length of new basis doesn't equal 
         *   TYPE_FLIP - new basis vector orientation differ from original one
         *   TYPE_QUADRANT_ROTATION - new basis is rotated by 90, 180, 270, or 360 degrees     
         *   TYPE_GENERAL_ROTATION - new basis is rotated by arbitrary angle
         *   TYPE_GENERAL_TRANSFORM - transformation can't be inversed
         */
        public virtual int Type
        {
            get
            {
                if (this._type != TYPE_UNKNOWN)
                {
                    return this._type;
                }

                int type = 0;

                if (_m00 * _m01 + _m10 * _m11 != 0.0)
                {
                    type |= TYPE_GENERAL_TRANSFORM;
                    return type;
                }

                if (_m02 != 0.0 || _m12 != 0.0)
                {
                    type |= TYPE_TRANSLATION;
                }
                else if (_m00 == 1.0 && _m11 == 1.0 && _m01 == 0.0 && _m10 == 0.0)
                {
                    type = TYPE_IDENTITY;
                    return type;
                }

                if (_m00 * _m11 - _m01 * _m10 < 0.0)
                {
                    type |= TYPE_FLIP;
                }

                double dx = _m00 * _m00 + _m10 * _m10;
                double dy = _m01 * _m01 + _m11 * _m11;
                if (dx != dy)
                {
                    type |= TYPE_GENERAL_SCALE;
                }
                else if (dx != 1.0)
                {
                    type |= TYPE_UNIFORM_SCALE;
                }

                if ((_m00 == 0.0 && _m11 == 0.0) ||
                    (_m10 == 0.0 && _m01 == 0.0 && (_m00 < 0.0 || _m11 < 0.0)))
                {
                    type |= TYPE_QUADRANT_ROTATION;
                }
                else if (_m01 != 0.0 || _m10 != 0.0)
                {
                    type |= TYPE_GENERAL_ROTATION;
                }

                return type;
            }
        }

        public virtual double GetScaleX()
        {
            return _m00;
        }

        public virtual double GetScaleY()
        {
            return _m11;
        }

        public virtual double GetShearX()
        {
            return _m01;
        }

        public virtual double GetShearY()
        {
            return _m10;
        }

        public virtual double GetTranslateX()
        {
            return _m02;
        }

        public virtual double GetTranslateY()
        {
            return _m12;
        }

        public virtual bool IsIdentity()
        {
            return Type == TYPE_IDENTITY;
        }

        public virtual void GetMatrix(double[] matrix)
        {
            matrix[0] = _m00;
            matrix[1] = _m10;
            matrix[2] = _m01;
            matrix[3] = _m11;
            if (matrix.Length > 4)
            {
                matrix[4] = _m02;
                matrix[5] = _m12;
            }
        }

        public virtual double GetDeterminant()
        {
            return _m00 * _m11 - _m01 * _m10;
        }

        public virtual void SetTransform(double m00, double m10, double m01, double m11, double m02, double m12)
        {
            _type = TYPE_UNKNOWN;
            this._m00 = m00;
            this._m10 = m10;
            this._m01 = m01;
            this._m11 = m11;
            this._m02 = m02;
            this._m12 = m12;
        }

        public virtual void SetTransform(AffineTransform t)
        {
            _type = t._type;
            SetTransform(t._m00, t._m10, t._m01, t._m11, t._m02, t._m12);
        }

        public virtual void SetToIdentity()
        {
            _type = TYPE_IDENTITY;
            _m00 = _m11 = 1.0;
            _m10 = _m01 = _m02 = _m12 = 0.0;
        }

        public virtual void SetToTranslation(double mx, double my)
        {
            _m00 = _m11 = 1.0;
            _m01 = _m10 = 0.0;
            _m02 = mx;
            _m12 = my;
            if (mx == 0.0 && my == 0.0)
            {
                _type = TYPE_IDENTITY;
            }
            else
            {
                _type = TYPE_TRANSLATION;
            }
        }

        public virtual void SetToScale(double scx, double scy)
        {
            _m00 = scx;
            _m11 = scy;
            _m10 = _m01 = _m02 = _m12 = 0.0;
            if (scx != 1.0 || scy != 1.0)
            {
                _type = TYPE_UNKNOWN;
            }
            else
            {
                _type = TYPE_IDENTITY;
            }
        }

        public virtual void SetToShear(double shx, double shy)
        {
            _m00 = _m11 = 1.0;
            _m02 = _m12 = 0.0;
            _m01 = shx;
            _m10 = shy;
            if (shx != 0.0 || shy != 0.0)
            {
                _type = TYPE_UNKNOWN;
            }
            else
            {
                _type = TYPE_IDENTITY;
            }
        }

        public virtual void SetToRotation(double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);
            if (Math.Abs(cos) < ZERO)
            {
                cos = 0.0;
                sin = sin > 0.0 ? 1.0 : -1.0;
            }
            else
                if (Math.Abs(sin) < ZERO)
            {
                sin = 0.0;
                cos = cos > 0.0 ? 1.0 : -1.0;
            }
            _m00 = _m11 = cos;
            _m01 = -sin;
            _m10 = sin;
            _m02 = _m12 = 0.0;
            _type = TYPE_UNKNOWN;
        }

        public virtual void SetToRotation(double angle, double px, double py)
        {
            SetToRotation(angle);
            _m02 = px * (1.0 - _m00) + py * _m10;
            _m12 = py * (1.0 - _m00) - px * _m10;
            _type = TYPE_UNKNOWN;
        }

        public static AffineTransform GetTranslateInstance(double mx, double my)
        {
            AffineTransform t = new AffineTransform();
            t.SetToTranslation(mx, my);
            return t;
        }

        public static AffineTransform GetScaleInstance(double scx, double scY)
        {
            AffineTransform t = new AffineTransform();
            t.SetToScale(scx, scY);
            return t;
        }

        public static AffineTransform GetShearInstance(double shx, double shy)
        {
            AffineTransform m = new AffineTransform();
            m.SetToShear(shx, shy);
            return m;
        }

        public static AffineTransform GetRotateInstance(double angle)
        {
            AffineTransform t = new AffineTransform();
            t.SetToRotation(angle);
            return t;
        }

        public static AffineTransform GetRotateInstance(double angle, double x, double y)
        {
            AffineTransform t = new AffineTransform();
            t.SetToRotation(angle, x, y);
            return t;
        }

        public virtual void Translate(double mx, double my)
        {
            Concatenate(GetTranslateInstance(mx, my));
        }

        public virtual void Scale(double scx, double scy)
        {
            Concatenate(GetScaleInstance(scx, scy));
        }

        public virtual void Shear(double shx, double shy)
        {
            Concatenate(GetShearInstance(shx, shy));
        }

        public virtual void Rotate(double angle)
        {
            Concatenate(GetRotateInstance(angle));
        }

        public virtual void Rotate(double angle, double px, double py)
        {
            Concatenate(GetRotateInstance(angle, px, py));
        }

        /** 
         * Multiply matrix of two AffineTransform objects 
         * @param t1 - the AffineTransform object is a multiplicand
         * @param t2 - the AffineTransform object is a multiplier
         * @return an AffineTransform object that is a result of t1 multiplied by matrix t2. 
         */
        AffineTransform Multiply(AffineTransform t1, AffineTransform t2)
        {
            return new AffineTransform(
                    t1._m00 * t2._m00 + t1._m10 * t2._m01,          // m00
                    t1._m00 * t2._m10 + t1._m10 * t2._m11,          // m01
                    t1._m01 * t2._m00 + t1._m11 * t2._m01,          // m10
                    t1._m01 * t2._m10 + t1._m11 * t2._m11,          // m11
                    t1._m02 * t2._m00 + t1._m12 * t2._m01 + t2._m02, // m02
                    t1._m02 * t2._m10 + t1._m12 * t2._m11 + t2._m12);// m12
        }

        public virtual void Concatenate(AffineTransform t)
        {
            SetTransform(Multiply(t, this));
        }

        public virtual void PreConcatenate(AffineTransform t)
        {
            SetTransform(Multiply(this, t));
        }

        public virtual AffineTransform CreateInverse()
        {
            double det = GetDeterminant();
            if (Math.Abs(det) < ZERO)
            {
                // awt.204=Determinant is zero
                throw new InvalidOperationException("awt.204");
            }
            return new AffineTransform(
                     _m11 / det, // m00
                    -_m10 / det, // m10
                    -_m01 / det, // m01
                     _m00 / det, // m11
                    (_m01 * _m12 - _m11 * _m02) / det, // m02
                    (_m10 * _m02 - _m00 * _m12) / det  // m12
            );
        }

        public virtual Point2D Transform(Point2D src, Point2D dst)
        {
            if (dst == null)
            {
                if (src is Point2D.Double)
                {
                    dst = new Point2D.Double();
                }
                else
                {
                    dst = new Point2D.Float();
                }
            }

            double x = src.GetX();
            double y = src.GetY();

            dst.SetLocation(x * _m00 + y * _m01 + _m02, x * _m10 + y * _m11 + _m12);
            return dst;
        }

        public virtual void Transform(Point2D[] src, int srcOff, Point2D[] dst, int dstOff, int length)
        {
            while (--length >= 0)
            {
                Point2D srcPoint = src[srcOff++];
                double x = srcPoint.GetX();
                double y = srcPoint.GetY();
                Point2D dstPoint = dst[dstOff];
                if (dstPoint == null)
                {
                    if (srcPoint is Point2D.Double)
                    {
                        dstPoint = new Point2D.Double();
                    }
                    else
                    {
                        dstPoint = new Point2D.Float();
                    }
                }
                dstPoint.SetLocation(x * _m00 + y * _m01 + _m02, x * _m10 + y * _m11 + _m12);
                dst[dstOff++] = dstPoint;
            }
        }

        public virtual void Transform(double[] src, int srcOff, double[] dst, int dstOff, int length)
        {
            int step = 2;
            if (src == dst && srcOff < dstOff && dstOff < srcOff + length * 2)
            {
                srcOff = srcOff + length * 2 - 2;
                dstOff = dstOff + length * 2 - 2;
                step = -2;
            }
            while (--length >= 0)
            {
                double x = src[srcOff + 0];
                double y = src[srcOff + 1];
                dst[dstOff + 0] = x * _m00 + y * _m01 + _m02;
                dst[dstOff + 1] = x * _m10 + y * _m11 + _m12;
                srcOff += step;
                dstOff += step;
            }
        }

        public virtual void Transform(float[] src, int srcOff, float[] dst, int dstOff, int length)
        {
            int step = 2;
            if (src == dst && srcOff < dstOff && dstOff < srcOff + length * 2)
            {
                srcOff = srcOff + length * 2 - 2;
                dstOff = dstOff + length * 2 - 2;
                step = -2;
            }
            while (--length >= 0)
            {
                float x = src[srcOff + 0];
                float y = src[srcOff + 1];
                dst[dstOff + 0] = (float)(x * _m00 + y * _m01 + _m02);
                dst[dstOff + 1] = (float)(x * _m10 + y * _m11 + _m12);
                srcOff += step;
                dstOff += step;
            }
        }

        public virtual void Transform(float[] src, int srcOff, double[] dst, int dstOff, int length)
        {
            while (--length >= 0)
            {
                float x = src[srcOff++];
                float y = src[srcOff++];
                dst[dstOff++] = x * _m00 + y * _m01 + _m02;
                dst[dstOff++] = x * _m10 + y * _m11 + _m12;
            }
        }

        public virtual void Transform(double[] src, int srcOff, float[] dst, int dstOff, int length)
        {
            while (--length >= 0)
            {
                double x = src[srcOff++];
                double y = src[srcOff++];
                dst[dstOff++] = (float)(x * _m00 + y * _m01 + _m02);
                dst[dstOff++] = (float)(x * _m10 + y * _m11 + _m12);
            }
        }

        public virtual Point2D DeltaTransform(Point2D src, Point2D dst)
        {
            if (dst == null)
            {
                if (src is Point2D.Double)
                {
                    dst = new Point2D.Double();
                }
                else
                {
                    dst = new Point2D.Float();
                }
            }

            double x = src.GetX();
            double y = src.GetY();

            dst.SetLocation(x * _m00 + y * _m01, x * _m10 + y * _m11);
            return dst;
        }

        public virtual void DeltaTransform(double[] src, int srcOff, double[] dst, int dstOff, int length)
        {
            while (--length >= 0)
            {
                double x = src[srcOff++];
                double y = src[srcOff++];
                dst[dstOff++] = x * _m00 + y * _m01;
                dst[dstOff++] = x * _m10 + y * _m11;
            }
        }

        public virtual Point2D InverseTransform(Point2D src, Point2D dst)
        {
            double det = GetDeterminant();
            if (Math.Abs(det) < ZERO)
            {
                // awt.204=Determinant is zero
                throw new InvalidOperationException("awt.204");
            }

            if (dst == null)
            {
                if (src is Point2D.Double)
                {
                    dst = new Point2D.Double();
                }
                else
                {
                    dst = new Point2D.Float();
                }
            }

            double x = src.GetX() - _m02;
            double y = src.GetY() - _m12;

            dst.SetLocation((x * _m11 - y * _m01) / det, (y * _m00 - x * _m10) / det);
            return dst;
        }

        public virtual void InverseTransform(double[] src, int srcOff, double[] dst, int dstOff, int length)
        {
            double det = GetDeterminant();
            if (Math.Abs(det) < ZERO)
            {
                // awt.204=Determinant is zero
                throw new InvalidOperationException("awt.204");
            }

            while (--length >= 0)
            {
                double x = src[srcOff++] - _m02;
                double y = src[srcOff++] - _m12;
                dst[dstOff++] = (x * _m11 - y * _m01) / det;
                dst[dstOff++] = (y * _m00 - x * _m10) / det;
            }
        }

        public virtual void InverseTransform(float[] src, int srcOff, float[] dst, int dstOff, int length)
        {
            float det = (float)GetDeterminant();
            if (Math.Abs(det) < ZERO)
            {
                // awt.204=Determinant is zero
                throw new InvalidOperationException("awt.204");
            }

            while (--length >= 0)
            {
                float x = src[srcOff++] - (float)_m02;
                float y = src[srcOff++] - (float)_m12;
                dst[dstOff++] = (x * (float)_m11 - y * (float)_m01) / det;
                dst[dstOff++] = (y * (float)_m00 - x * (float)_m10) / det;
            }
        }

        public virtual object Clone()
        {
            return new AffineTransform(this);
        }

        //    public Shape createTransformedShape(Shape src) {
        //        if (src == null) {
        //            return null;
        //        }
        //        if (src is GeneralPath) {
        //            return ((GeneralPath)src).createTransformedShape(this);
        //        }
        //        PathIterator path = src.GetPathIterator(this);
        //        GeneralPath dst = new GeneralPath(path.GetWindingRule());
        //        dst.append(path, false);
        //        return dst;
        //    }

        public override string ToString()
        {
            return "Transform: [[" + _m00 + ", " + _m01 + ", " + _m02 + "], ["
                    + _m10 + ", " + _m11 + ", " + _m12 + "]]";
        }
    }
}
