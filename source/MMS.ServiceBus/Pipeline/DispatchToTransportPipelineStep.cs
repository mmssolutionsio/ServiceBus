//-------------------------------------------------------------------------------
// <copyright file="DispatchToTransportPipelineStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Threading.Tasks;

    public class DispatchToTransportPipelineStep : IOutgoingTransportPipelineStep
    {
        private readonly MessageSender sender;

        public DispatchToTransportPipelineStep(MessageSender sender)
        {
            this.sender = sender;
        }

        public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            await this.sender.SendAsync(context.TransportMessage, (SendOptions)context.Options);

            await next();
        }
    }
}