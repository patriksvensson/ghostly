using System.ComponentModel;
using System.Globalization;
using Spectre.IO;

namespace Ghostly.Tools.Utilities
{
    public sealed class FilePathConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                return new FilePath(stringValue);
            }

            throw new NotSupportedException("Can't convert value to directory path.");
        }
    }
}
