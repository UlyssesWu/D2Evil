namespace D2Evil
{
    public struct L2PointF
    {
        public L2PointF(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public L2PointF(double x, double y)
        {
            this.X = (float)x;
            this.Y = (float)y;
        }

        public L2PointF(L2PointF pt)
        {
            this.X = pt.X;
            this.Y = pt.Y;
        }

        public void SetPoint(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public void SetPoint(L2PointF pt)
        {
            this.X = pt.X;
            this.Y = pt.Y;
        }

        public float X { get; set; }

        public float Y { get; set; }
    }

    public struct L2Point
    {
        public L2Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public L2Point(L2Point pt)
        {
            this.X = pt.X;
            this.Y = pt.Y;
        }

        public void SetPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public void SetPoint(L2Point pt)
        {
            this.X = pt.X;
            this.Y = pt.Y;
        }

        public int X { get; set; }

        public int Y { get; set; }
    }
}
