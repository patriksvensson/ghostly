namespace Ghostly.Domain.Messages
{
    public sealed class CategoryCreated
    {
        public Category Category { get; }

        public CategoryCreated(Category category)
        {
            Category = category;
        }
    }

    public sealed class CategoryEdited
    {
        public Category Category { get; }
        public int Unread { get; }

        public CategoryEdited(Category category)
        {
            Category = category;
        }
    }
}
