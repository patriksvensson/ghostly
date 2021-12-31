namespace Ghostly.Uwp.Utilities
{
    internal static class CompilationInfo
    {
#if ARCH_X86
        public const string Architecture = "x86";
#elif ARCH_X64
        public const string Architecture = "x64";
#elif ARCH_ARM32
        public const string Architecture = "ARM32";
#elif ARCH_ARM64
        public const string Architecture = "ARM64";
#else
        public const string Architecture = "Unknown";
#endif
    }
}
