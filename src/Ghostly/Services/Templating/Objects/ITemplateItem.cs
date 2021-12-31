namespace Ghostly.Services.Templating.Objects
{
    public interface ITemplateItem : ITemplateComment
    {
        bool IsReview { get; }
        bool ShowBody { get; }
    }
}
