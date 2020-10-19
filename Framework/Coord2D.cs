using Microsoft.Xna.Framework;
using System.Globalization;

namespace SunbirdMB.Framework
{
    public struct Coord2D
    {
        public static Coord2D Zero { get; }

        private int x;
        private int y;

        public Coord2D(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public static Coord2D operator +(Coord2D c1, Coord2D c2)
        {
            return new Coord2D(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static Coord2D operator -(Coord2D c1, Coord2D c2)
        {
            return new Coord2D(c1.X - c2.X, c1.Y - c2.Y);
        }

        public static Coord2D operator *(Coord2D c1, Coord2D c2)
        {
            return new Coord2D(c1.X * c2.X, c1.Y * c2.Y);
        }

        public static Coord2D operator *(Coord2D c1, int i)
        {
            return new Coord2D(c1.X * i, c1.Y * i);
        }

        public static Coord2D operator /(Coord2D c1, Coord2D c2)
        {
            return new Coord2D(c1.X / c2.X, c1.Y / c2.Y);
        }

        public static bool operator ==(Coord2D left, Coord2D right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Coord2D left, Coord2D right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return unchecked(x ^ y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Coord2D)) return false;
            Coord2D comp = (Coord2D)obj;
            return comp.X == this.X && comp.Y == this.Y;
        }

        public override string ToString()
        {
            return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) + "}";
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }

    }
}
