using System.Collections.Generic;

namespace Ghostly.Core
{
    public static class Emoji
    {
        private static readonly List<EmojiInfo> _all;
        public static IReadOnlyList<EmojiInfo> Supported => _all;

        static Emoji()
        {
            _all = new List<EmojiInfo>
            {
                EmojiInfo.Define("🥳", "Partying face", EmojiCategory.Faces),
                EmojiInfo.Define("☠", "Skull and crossbones", EmojiCategory.Faces),
                EmojiInfo.Define("👻", "Ghost", EmojiCategory.Faces),
                EmojiInfo.Define("👽", "Alien", EmojiCategory.Faces),
                EmojiInfo.Define("👾", "Alien monster", EmojiCategory.Faces),
                EmojiInfo.Define("🤖", "Robot", EmojiCategory.Faces),
                EmojiInfo.Define("💩", "Pile of poo", EmojiCategory.Faces),
                EmojiInfo.Define("🐱‍👤", "Ninja cat", EmojiCategory.Faces, deprecated: true),
                EmojiInfo.Define("🐱‍🏍", "Stunt cat", EmojiCategory.Faces, deprecated: true),
                EmojiInfo.Define("🐱‍💻", "Hacker cat", EmojiCategory.Faces, deprecated: true),
                EmojiInfo.Define("🐱‍🐉", "Dino cat", EmojiCategory.Faces, deprecated: true),
                EmojiInfo.Define("🐱‍👓", "Hipster cat", EmojiCategory.Faces, deprecated: true),
                EmojiInfo.Define("🐱‍🚀", "Astro cat", EmojiCategory.Faces, deprecated: true),
                EmojiInfo.Define("🙈", "See no evil", EmojiCategory.Faces),
                EmojiInfo.Define("🙉", "Hear no evil", EmojiCategory.Faces),
                EmojiInfo.Define("🙊", "Speak no evil", EmojiCategory.Faces),
                EmojiInfo.Define("🦀", "Crab", EmojiCategory.Animals),
                EmojiInfo.Define("🐠", "Fish", EmojiCategory.Animals),
                EmojiInfo.Define("🦆", "Duck", EmojiCategory.Animals),
                EmojiInfo.Define("🎈", "Balloon", EmojiCategory.Objects),
                EmojiInfo.Define("🎉", "Party popper", EmojiCategory.Objects),
                EmojiInfo.Define("🎁", "Presemt", EmojiCategory.Food),
                EmojiInfo.Define("🍰", "Cake", EmojiCategory.Food),
                EmojiInfo.Define("🎂", "Birthday cake", EmojiCategory.Food),
                EmojiInfo.Define("👔", "Neck tie", EmojiCategory.Objects),
                EmojiInfo.Define("💎", "Diamond", EmojiCategory.Objects),
                EmojiInfo.Define("🕹", "Joystick", EmojiCategory.Objects),
                EmojiInfo.Define("🧩", "Puzzle piece", EmojiCategory.Objects),
                EmojiInfo.Define("🎵", "Musical note", EmojiCategory.Objects),
                EmojiInfo.Define("⚔", "Crossed swords", EmojiCategory.Objects),
                EmojiInfo.Define("💣", "Bomb", EmojiCategory.Objects),
                EmojiInfo.Define("📕", "Red book", EmojiCategory.Objects),
                EmojiInfo.Define("📗", "Green book", EmojiCategory.Objects),
                EmojiInfo.Define("📘", "Blue book", EmojiCategory.Objects),
                EmojiInfo.Define("📙", "Orange book", EmojiCategory.Objects),
                EmojiInfo.Define("📚", "Books", EmojiCategory.Objects),
                EmojiInfo.Define("📃", "Page with curl", EmojiCategory.Objects),
                EmojiInfo.Define("📜", "Scroll", EmojiCategory.Objects),
                EmojiInfo.Define("📦", "Package", EmojiCategory.Objects),
                EmojiInfo.Define("📁", "Folder", EmojiCategory.Objects),
                EmojiInfo.Define("📂", "Open folder", EmojiCategory.Objects),
                EmojiInfo.Define("🗂", "Folders", EmojiCategory.Objects),
                EmojiInfo.Define("📌", "Pin", EmojiCategory.Objects),
                EmojiInfo.Define("⌛", "Hour glass", EmojiCategory.Objects),
                EmojiInfo.Define("⌚", "Wrist watch", EmojiCategory.Objects),
                EmojiInfo.Define("⏰", "Alarm", EmojiCategory.Objects),
                EmojiInfo.Define("🏠", "House", EmojiCategory.Places),
                EmojiInfo.Define("🏢", "Building", EmojiCategory.Places),
                EmojiInfo.Define("⭐", "Star", EmojiCategory.Symbols),
                EmojiInfo.Define("🔥", "Fire", EmojiCategory.Symbols),
                EmojiInfo.Define("⚡", "Lightning", EmojiCategory.Symbols),
                EmojiInfo.Define("❤", "Red heart", EmojiCategory.Symbols),
                EmojiInfo.Define("🧡", "Orange", EmojiCategory.Symbols),
                EmojiInfo.Define("💛", "Yellow", EmojiCategory.Symbols),
                EmojiInfo.Define("💚", "Green", EmojiCategory.Symbols),
                EmojiInfo.Define("💙", "Blue", EmojiCategory.Symbols),
                EmojiInfo.Define("💜", "Purple", EmojiCategory.Symbols),
                EmojiInfo.Define("🤎", "Brown", EmojiCategory.Symbols),
                EmojiInfo.Define("🖤", "Black", EmojiCategory.Symbols),
                EmojiInfo.Define("💔", "Broken heart", EmojiCategory.Symbols),
                EmojiInfo.Define("💌", "Love letter", EmojiCategory.Symbols),
                EmojiInfo.Define("💤", "Snooze", EmojiCategory.Symbols),
                EmojiInfo.Define("🔇", "Muted speaker", EmojiCategory.Symbols),
                EmojiInfo.Define("🔕", "Bell with slash", EmojiCategory.Symbols),
                EmojiInfo.Define("❌", "Cross mark", EmojiCategory.Symbols),
                EmojiInfo.Define("⭕", "Circle", EmojiCategory.Symbols),
                EmojiInfo.Define("❓", "Question mark", EmojiCategory.Symbols),
                EmojiInfo.Define("‼", "Double exclamation marks", EmojiCategory.Symbols),
                EmojiInfo.Define("⁉", "Exclamation question mark", EmojiCategory.Symbols),
                EmojiInfo.Define("💯", "Hundred points", EmojiCategory.Symbols),
                EmojiInfo.Define("✔", "Check mark", EmojiCategory.Symbols),
                EmojiInfo.Define("🔴", "Red circle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟠", "Orange circle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟡", "Yellow circle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟢", "Green circle", EmojiCategory.Symbols),
                EmojiInfo.Define("🔵", "Blue circle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟣", "Purple circle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟤", "Brown circle", EmojiCategory.Symbols),
                EmojiInfo.Define("⚫", "Black circle", EmojiCategory.Symbols),
                EmojiInfo.Define("⚪", "White circle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟥", "Red rectangle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟧", "Orange rectangle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟨", "Yellow rectangle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟩", "Green rectangle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟦", "Blue rectangle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟪", "Purple rectangle", EmojiCategory.Symbols),
                EmojiInfo.Define("🟫", "Brown rectangle", EmojiCategory.Symbols),
                EmojiInfo.Define("⬛", "Black rectangle", EmojiCategory.Symbols),
                EmojiInfo.Define("⬜", "White rectangle", EmojiCategory.Symbols),
                EmojiInfo.Define("💭", "Thought bubble", EmojiCategory.Symbols),
                EmojiInfo.Define("🗯", "Anger bubble", EmojiCategory.Symbols),
                EmojiInfo.Define("💬", "Right speech bubble", EmojiCategory.Symbols),
                EmojiInfo.Define("🗨", "Left speech bubble", EmojiCategory.Symbols),
                EmojiInfo.Define("👁‍🗨", "Eye in speech bubble", EmojiCategory.Symbols),
            };
        }
    }

    public sealed class EmojiInfo
    {
        public string Name { get; set; }
        public EmojiCategory Category { get; set; }
        public string Value { get; set; }
        public bool Deprecated { get; set; }

        public static EmojiInfo Define(string value, string name, EmojiCategory category, bool deprecated = false)
        {
            return new EmojiInfo
            {
                Name = name,
                Category = category,
                Value = value,
                Deprecated = deprecated,
            };
        }
    }

    public enum EmojiCategory
    {
        Faces,
        Animals,
        Objects,
        Places,
        Food,
        Symbols,
    }
}
