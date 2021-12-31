namespace Ghostly
{
    public static class BooleanExtensions
    {
        public static string ToYesNo(this bool value)
        {
            return value ? "yes" : "no";
        }
    }
}
