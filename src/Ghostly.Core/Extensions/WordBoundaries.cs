using System;

namespace Ghostly.Core
{
    public struct WordBoundaries : IEquatable<WordBoundaries>
    {
        public int Start { get; set; }
        public int End { get; set; }
        public bool Valid { get; }

        public WordBoundaries(int start, int end)
        {
            Start = start;
            End = end;
            Valid = true;
        }

        public override bool Equals(object obj)
        {
            if (obj is WordBoundaries other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public bool Equals(WordBoundaries other)
        {
            return Start == other.Start &&
                End == other.End &&
                Valid == other.Valid;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + Start.GetHashCode();
                hash = (hash * 7) + End.GetHashCode();
                hash = (hash * 7) + Valid.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(WordBoundaries left, WordBoundaries right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WordBoundaries left, WordBoundaries right)
        {
            return !(left == right);
        }
    }
}
