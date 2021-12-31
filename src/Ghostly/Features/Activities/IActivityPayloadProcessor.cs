using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data.Models;

namespace Ghostly.Features.Activities
{
    public interface IActivityPayloadProcessor
    {
        ActivityKind Kind { get; }

        ActivityPayload Deserialize(string payload);
        Task<bool> Process(ActivityPayload activity, CancellationToken cancellationToken);
    }
}
