namespace Ghostly.Domain
{
    public sealed class CategoryOrder
    {
        public int Id { get; }
        public int SortOrder { get; }

        public CategoryOrder(int id, int sortorder)
        {
            Id = id;
            SortOrder = sortorder;
        }
    }
}
