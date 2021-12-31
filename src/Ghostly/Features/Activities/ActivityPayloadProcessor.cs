using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data.Models;
using Newtonsoft.Json;

namespace Ghostly.Features.Activities
{
    public abstract class ActivityPayloadProcessor<T> : IActivityPayloadProcessor
        where T : ActivityPayload
    {
        public abstract ActivityKind Kind { get; }

        ActivityPayload IActivityPayloadProcessor.Deserialize(string payload)
        {
            return Deserialize(payload);
        }

        async Task<bool> IActivityPayloadProcessor.Process(ActivityPayload payload, CancellationToken cancellationToken)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.Kind != Kind)
            {
                throw new InvalidOperationException("Activity kind mismatch in processor.");
            }

            return await Process((T)payload);
        }

        public T Deserialize(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload);
        }

        protected abstract Task<bool> Process(T activity);
    }
}
