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
        private readonly MessagePublisher publisher;

        public DispatchToTransportPipelineStep(MessageSender sender, MessagePublisher publisher)
        {
            this.publisher = publisher;
            this.sender = sender;
        }

        public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            var sendOptions = context.Options as SendOptions;
            if (sendOptions != null)
            {
                await this.sender.SendAsync(context.TransportMessage, sendOptions);
            }

            var publishOptions = context.Options as PublishOptions;
            if (publishOptions != null)
            {
                await this.publisher.PublishAsync(context.TransportMessage, publishOptions);
            }

            await next();
        }
    }
}