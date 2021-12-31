using System;

namespace Ghostly.Domain
{
    public sealed class Culture : IEquatable<Culture>
    {
        public string Name { get; }
        public string CultureCode { get; }
        public string LanguageCode { get; }

        public Culture(string name, string cultureCode, string languageCode)
        {
            Name = name;
            CultureCode = cultureCode;
            LanguageCode = languageCode;
        }

        public override bool Equals(object obj)
        {
            if (obj is Culture other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public bool Equals(Culture other)
        {
            if (other == null)
            {
                return false;
            }

            return CultureCode.Equals(other.CultureCode, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return CultureCode.GetHashCode();
        }
    }
}
