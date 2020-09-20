using Microsoft.Xna.Framework;
using System.Globalization;

namespace SunbirdMB.Framework
{
    public struct Dimension
    {
        public static Dimension Zero { get; }

        private int x;
        private int y;
        private int z;

        public Dimension(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
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

        public int Z
        {
            get
            {
                return z;
            }
            set
            {
                z = value;
            }
        }

        public static Dimension operator +(Dimension d1, Dimension d2)
        {
            return new Dimension(d1.X + d2.X, d1.Y + d2.Y, d1.Z + d2.Z);
        }

        public static Dimension operator -(Dimension d1, Dimension d2)
        {
            return new Dimension(d1.X - d2.X, d1.Y - d2.Y, d1.Z - d2.Z);
        }

        public static Dimension operator *(Dimension d1, Dimension d2)
        {
            return new Dimension(d1.X * d2.X, d1.Y * d2.Y, d1.Z * d2.Z);
        }

        public static Dimension operator *(Dimension d1, int i)
        {
            return new Dimension(d1.X * i, d1.Y * i, d1.Z * i);
        }

        public static Dimension operator /(Dimension d1, Dimension d2)
        {
            return new Dimension(d1.X / d2.X, d1.Y / d2.Y, d1.Z / d2.Z);
        }

        public static bool operator ==(Dimension left, Dimension right)
        {
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        }

        public static bool operator !=(Dimension left, Dimension right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + x.GetHashCode();
                hash = hash * 23 + y.GetHashCode();
                hash = hash * 23 + z.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Dimension)) return false;
            Dimension comp = (Dimension)obj;
            return comp.X == this.X && comp.Y == this.Y && comp.Z == this.Z;
        }

        public override string ToString()
        {
            return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) + ",Z=" + Z.ToString(CultureInfo.CurrentCulture) + "}";
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

    }
}
