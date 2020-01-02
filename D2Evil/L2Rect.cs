namespace D2Evil
{
    public struct L2RectF
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public L2RectF(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public L2RectF(double x, double y, double width, double height)
        {
            this.X = (float)x;
            this.Y = (float)y;
            this.Width = (float)width;
            this.Height = (float)height;
        }

        public void Expand(float w, float h)
        {
            X -= w;
            Y -= h;
            Width += w * 2f;
            Height += h * 2f;
        }
        public bool Contains(float x, float y)
        {
            return this.X <= x && this.Y <= y && x <= this.X + this.Width && y <= this.Y + this.Height;
        }

        public void Add(float x1, float y1)
        {
            if (X < x1)
            {
                if (X + Width < x1)
                {
                    Width = x1 - X;
                }
            }
            else
            {
                Width = X + Width - x1;
                X = x1;
            }
            if (Y < y1)
            {
                if (Y + Height < y1)
                {
                    Height = y1 - Y;
                }
            }
            else
            {
                Height = Y + Height - y1;
                Y = y1;
            }
        }

        public L2RectF(L2RectF r)
        {
            this.X = r.X;
            this.Y = r.Y;
            this.Width = r.Width;
            this.Height = r.Height;
        }

        public void SetRect(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public void SetRect(L2RectF r)
        {
            this.X = r.X;
            this.Y = r.Y;
            this.Width = r.Width;
            this.Height = r.Height;
        }

        public float CenterX => this.X + 0.5f * this.Width;

        public float CenterY => this.Y + 0.5f * this.Height;
        public float Right => this.X + this.Width;

        public float Bottom => this.Y + this.Height;

        public override string ToString()
        {
            return string.Concat(X, " , ", Y, " , ", Width, " , ", Height);
        }
    }

    public struct L2Rect
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public L2Rect(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
        public L2Rect(L2Rect r)
        {
            this.X = r.X;
            this.Y = r.Y;
            this.Width = r.Width;
            this.Height = r.Height;
        }

        public bool Contains(int x, int y)
        {
            return this.X <= x && this.Y <= y && x <= this.X + this.Width && y <= this.Y + this.Height;
        }

        public void Add(int x1, int y1)
        {
            if (X < x1)
            {
                if (X + Width < x1)
                {
                    Width = x1 - X;
                }
            }
            else
            {
                Width = X + Width - x1;
                X = x1;
            }
            if (Y < y1)
            {
                if (Y + Height < y1)
                {
                    Height = y1 - Y;
                }
            }
            else
            {
                Height = Y + Height - y1;
                Y = y1;
            }
        }

        public void SetRect(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public void SetRect(L2Rect r)
        {
            this.X = r.X;
            this.Y = r.Y;
            this.Width = r.Width;
            this.Height = r.Height;
        }

        public float CenterX => this.X + 0.5f * this.Width;

        public float CenterY => this.Y + 0.5f * this.Height;
        public int Right => this.X + this.Width;

        public int Bottom => this.Y + this.Height;

        public override string ToString()
        {
            return string.Concat(X, " , ", Y, " , ", Width, " , ", Height);
        }
    }

}
