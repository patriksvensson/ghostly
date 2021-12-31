using System;
using System.Globalization;

namespace Ghostly.Core
{
    public struct ColorRepresentation : IEquatable<ColorRepresentation>
    {
        public byte Alpha { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public static ColorRepresentation White => new ColorRepresentation(255, 255, 255, 255);

        public ColorRepresentation(byte alpha, byte red, byte green, byte blue)
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

        public override bool Equals(object obj)
        {
            if (obj is ColorRepresentation color)
            {
                return Equals(color);
            }

            return false;
        }

        public bool Equals(ColorRepresentation other)
        {
            return
                Alpha == other.Alpha && Red == other.Red &&
                Green == other.Green && Blue == other.Blue;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + Alpha.GetHashCode();
                hash = (hash * 23) + Red.GetHashCode();
                hash = (hash * 23) + Green.GetHashCode();
                hash = (hash * 23) + Blue.GetHashCode();
                return hash;
            }
        }

        public static ColorRepresentation ParseHex(string hex)
        {
            hex = hex ?? string.Empty;
            hex = hex.Replace("#", string.Empty);

            if (string.IsNullOrWhiteSpace(hex))
            {
                return White;
            }

            if (hex.Length == 8)
            {
                return new ColorRepresentation
                {
                    Alpha = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16),
                    Red = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16),
                    Green = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16),
                    Blue = (byte)Convert.ToUInt32(hex.Substring(6, 2), 16),
                };
            }
            else if (hex.Length == 6)
            {
                return new ColorRepresentation
                {
                    Alpha = 255,
                    Red = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16),
                    Green = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16),
                    Blue = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16),
                };
            }
            else if (hex.Length == 4)
            {
                return new ColorRepresentation
                {
                    Alpha = (byte)Convert.ToUInt32(new string(hex[0], 2), 16),
                    Red = (byte)Convert.ToUInt32(new string(hex[1], 2), 16),
                    Green = (byte)Convert.ToUInt32(new string(hex[2], 2), 16),
                    Blue = (byte)Convert.ToUInt32(new string(hex[3], 2), 16),
                };
            }
            else if (hex.Length == 3)
            {
                return new ColorRepresentation
                {
                    Alpha = 255,
                    Red = (byte)Convert.ToUInt32(new string(hex[0], 2), 16),
                    Green = (byte)Convert.ToUInt32(new string(hex[1], 2), 16),
                    Blue = (byte)Convert.ToUInt32(new string(hex[2], 2), 16),
                };
            }
            else
            {
                throw new InvalidOperationException($"Invalid color {hex}.");
            }
        }

        public string ToHex()
        {
            var a = Alpha.ToString("X2", CultureInfo.InvariantCulture);
            var r = Red.ToString("X2", CultureInfo.InvariantCulture);
            var g = Green.ToString("X2", CultureInfo.InvariantCulture);
            var b = Blue.ToString("X2", CultureInfo.InvariantCulture);
            return $"#{a}{r}{g}{b}";
        }

        public static bool operator ==(ColorRepresentation color1, ColorRepresentation color2)
        {
            return color1.Equals(color2);
        }

        public static bool operator !=(ColorRepresentation color1, ColorRepresentation color2)
        {
            return !color1.Equals(color2);
        }
    }
}
