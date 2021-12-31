using System;

namespace Ghostly.Core
{
    public struct Position : IEquatable<Position>
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position color)
            {
                return Equals(color);
            }

            return false;
        }

        public bool Equals(Position other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + X.GetHashCode();
                hash = (hash * 23) + Y.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Position pos1, Position pos2)
        {
            return pos1.Equals(pos2);
        }

        public static bool operator !=(Position pos1, Position pos2)
        {
            return !pos1.Equals(pos2);
        }
    }
}
