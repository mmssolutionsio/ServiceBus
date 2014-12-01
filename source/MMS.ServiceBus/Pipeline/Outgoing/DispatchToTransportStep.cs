//-------------------------------------------------------------------------------
// <copyright file="DispatchToTransportStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Threading.Tasks;

    public class DispatchToTransportStep : IOutgoingTransportStep
    {
        private readonly MessageSender sender;
        private readonly MessagePublisher publisher;

        public DispatchToTransportStep(MessageSender sender, MessagePublisher publisher)
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