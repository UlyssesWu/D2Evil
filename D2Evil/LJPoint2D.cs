//This is a port of java.awt.geom.Point2D
using System;

namespace FreeLive
{
    public abstract class Point2D
    {
        public class Float : Point2D
        {

            public float X;
            public float Y;

            public Float()
            {
            }

            public Float(float x, float y)
            {
                this.X = x;
                this.Y = y;
            }

            public override double GetX()
            {
                return X;
            }

            public override double GetY()
            {
                return Y;
            }

            public virtual void SetLocation(float x, float y)
            {
                this.X = x;
                this.Y = y;
            }

            public override void SetLocation(double x, double y)
            {
                this.X = (float)x;
                this.Y = (float)y;
            }

            public override string ToString()
            {
                return "Point2D:[x=" + X + ",y=" + Y + "]";
            }
        }

        public class Double : Point2D
        {

            public double X;
            public double Y;

            public Double()
            {
            }

            public Double(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }

            public override double GetX()
            {
                return X;
            }

            public override double GetY()
            {
                return Y;
            }

            public override void SetLocation(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }

            public override string ToString()
            {
                return "Point2D: [x=" + X + ",y=" + Y + "]";
            }
        }

        protected Point2D()
        {
        }

        public abstract double GetX();

        public abstract double GetY();

        public abstract void SetLocation(double x, double y);

        public virtual void SetLocation(Point2D p)
        {
            SetLocation(p.GetX(), p.GetY());
        }

        public static double DistanceSq(double x1, double y1, double x2, double y2)
        {
            x2 -= x1;
            y2 -= y1;
            return x2 * x2 + y2 * y2;
        }

        public virtual double DistanceSq(double px, double py)
        {
            return Point2D.DistanceSq(GetX(), GetY(), px, py);
        }

        public virtual double DistanceSq(Point2D p)
        {
            return Point2D.DistanceSq(GetX(), GetY(), p.GetX(), p.GetY());
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(DistanceSq(x1, y1, x2, y2));
        }

        public virtual double Distance(double px, double py)
        {
            return Math.Sqrt(DistanceSq(px, py));
        }

        public virtual double Distance(Point2D p)
        {
            return Math.Sqrt(DistanceSq(p));
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (obj is Point2D)
            {
                Point2D p = (Point2D)obj;
                return GetX() == p.GetX() && GetY() == p.GetY();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return GetX().GetHashCode() + GetY().GetHashCode();
        }
    }
}
