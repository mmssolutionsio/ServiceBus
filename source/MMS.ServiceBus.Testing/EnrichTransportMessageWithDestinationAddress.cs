using MMS.ServiceBus.Pipeline;

namespace MMS.ServiceBus.Testing
{
    using System;
    using System.Threading.Tasks;

    public class EnrichTransportMessageWithDestinationAddress : IOutgoingTransportPipelineStep
    {
        public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            var sendOptions = context.Options as SendOptions;
            if (sendOptions != null)
            {
                context.TransportMessage.Headers[AcceptanceTestHeaders.Destination] = sendOptions.Destination;
            }
            await next();
        }
    }
}