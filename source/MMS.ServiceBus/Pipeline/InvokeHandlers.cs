using System;
using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public class InvokeHandlers : IIncomingLogicalPipelineStep
    {
        public async Task Invoke(IncomingLogicalContext context, IBus bus, Func<Task> next)
        {
            var messageHandler = context.Handler;

            await messageHandler.Invocation(messageHandler.Instance, context.LogicalMessage.Instance);

            await next();
        }
    }
}