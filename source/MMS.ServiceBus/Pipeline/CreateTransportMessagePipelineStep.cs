//-------------------------------------------------------------------------------
// <copyright file="CreateTransportMessagePipelineStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Threading.Tasks;

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