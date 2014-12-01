//-------------------------------------------------------------------------------
// <copyright file="InvokeHandlers.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
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