using Microsoft.Xna.Framework;
using System.Globalization;

namespace SunbirdMB.Framework
{
    public struct Coord3D
    {
        public static Coord3D Zero { get; }

        private int x;
        private int y;
        private int z;

        public Coord3D(Coord2D coord2D, int z)
        {
            this.x = coord2D.X;
            this.y = coord2D.Y;
            this.z = z;
        }

        public Coord3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public int Z
        {
            get { return z; }
            set { z = value; }
        }

        public static Coord3D operator +(Coord3D c1, Coord3D c2)
        {
            return new Coord3D(c1.X + c2.X, c1.Y + c2.Y, c1.Z + c2.Z);
        }

        public static Coord3D operator -(Coord3D c1, Coord3D c2)
        {
            return new Coord3D(c1.X - c2.X, c1.Y - c2.Y, c1.Z - c2.Z);
        }

        public static Coord3D operator *(Coord3D c1, Coord3D c2)
        {
            return new Coord3D(c1.X * c2.X, c1.Y * c2.Y, c1.Z * c2.Z);
        }

        public static Coord3D operator *(Coord3D c1, int i)
        {
            return new Coord3D(c1.X * i, c1.Y * i, c1.Z * i);
        }

        public static Coord3D operator /(Coord3D c1, Coord3D c2)
        {
            return new Coord3D(c1.X / c2.X, c1.Y / c2.Y, c1.Z / c2.Z);
        }

        public static bool operator ==(Coord3D left, Coord3D right)
        {
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        }

        public static bool operator !=(Coord3D left, Coord3D right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return unchecked(x ^ y ^ z);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Coord3D)) return false;
            Coord3D comp = (Coord3D)obj;
            return comp.X == this.X && comp.Y == this.Y && comp.Z == this.Z;
        }

        public override string ToString()
        {
            return $"{{X = {X}, Y = {Y}, Z = {Z}}}";
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

    }
}
