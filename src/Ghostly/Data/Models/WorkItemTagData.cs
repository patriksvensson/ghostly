namespace Ghostly.Data.Models
{
    public sealed class WorkItemTagData : EntityData
    {
        public int WorkItemId { get; set; }
        public WorkItemData WorkItem { get; set; }
        public int TagId { get; set; }
        public TagData Tag { get; set; }
    }
}
