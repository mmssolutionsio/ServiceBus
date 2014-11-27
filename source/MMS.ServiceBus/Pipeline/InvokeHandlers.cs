namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    using System;
    using System.Threading.Tasks;

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