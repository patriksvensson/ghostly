using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data.Models;
using Newtonsoft.Json;

namespace Ghostly.Features.Activities
{
    public sealed class ActivityHelper
    {
        private readonly IEnumerable<IActivityPayloadProcessor> _processors;

        public ActivityHelper(IEnumerable<IActivityPayloadProcessor> processors)
        {
            _processors = processors;
        }

        public static string Serialize(ActivityPayload activity)
        {
            return JsonConvert.SerializeObject(activity);
        }

        public ActivityPayload Deserialize(ActivityKind kind, string payload)
        {
            return FindProcessor(kind).Deserialize(payload);
        }

        public async Task<bool> Process(ActivityPayload activity, CancellationToken cancellationToken)
        {
            if (activity is null)
            {
                throw new System.ArgumentNullException(nameof(activity));
            }

            var processor = FindProcessor(activity.Kind);
            if (processor == null)
            {
                return false;
            }

            return await processor.Process(activity, cancellationToken);
        }

        private IActivityPayloadProcessor FindProcessor(ActivityKind kind)
        {
            return _processors.SingleOrDefault(p => p.Kind == kind);
        }
    }
}
