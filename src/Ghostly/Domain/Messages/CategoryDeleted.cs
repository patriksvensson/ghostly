namespace Ghostly.Domain.Messages
{
    public sealed class CategoryDeleted
    {
        public Category Category { get; }

        public CategoryDeleted(Category category)
        {
            Category = category;
        }
    }
}
