namespace Ghostly
{
    public static class NullableExtensions
    {
        public static T GetSafeValue<T>(this T? value, T defaultValue = default)
            where T : struct
        {
            return value ?? defaultValue;
        }
    }
}
