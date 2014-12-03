namespace MMS.ServiceBus.Testing
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;
    using Pipeline.Outgoing;

    public class EnrichTransportMessageWithDestinationAddress : IOutgoingTransportStep
    {
        public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            var sendOptions = context.Options as SendOptions;
            if (sendOptions != null)
            {
                context.OutgoingTransportMessage.Headers[AcceptanceTestHeaders.Destination] = sendOptions.Destination;
            }

            await next();
        }
    }
}