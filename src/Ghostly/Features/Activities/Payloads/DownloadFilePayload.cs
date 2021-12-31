using Ghostly.Data.Models;

namespace Ghostly.Features.Activities.Payloads
{
    public sealed class DownloadFilePayload : ActivityPayload
    {
        public override ActivityCategory Category => ActivityCategory.Continuous;
        public override ActivityKind Kind => ActivityKind.DownloadFile;
        public override ActivityConstraint Contstraint => ActivityConstraint.RequiresInternetConnection;

        public string Url { get; set; }
        public string Path { get; set; }
        public string Filename { get; set; }
    }
}
