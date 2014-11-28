using System;
using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public class CreateTransportMessagePipelineStep : IOutgoingLogicalPipelineStep
    {
        public async Task Invoke(OutgoingLogicalContext context, Func<Task> next)
        {
            var message = new TransportMessage();

            context.Set(message);

            await next();
        }
    }
}