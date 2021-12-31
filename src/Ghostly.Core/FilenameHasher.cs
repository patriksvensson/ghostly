namespace Ghostly.Core
{
    public static class FilenameHasher
    {
        public static ulong Calculate(string read)
        {
            if (read is null)
            {
                throw new System.ArgumentNullException(nameof(read));
            }

            var hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }

            return hashedValue;
        }
    }
}
