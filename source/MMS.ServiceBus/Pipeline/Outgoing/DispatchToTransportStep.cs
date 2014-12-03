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
        private readonly ISendMessages sender;
        private readonly IPublishMessages publisher;

        public DispatchToTransportStep(ISendMessages sender, IPublishMessages publisher)
        {
            this.publisher = publisher;
            this.sender = sender;
        }

        public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            var sendOptions = context.Options as SendOptions;
            if (sendOptions != null)
            {
                await this.sender.SendAsync(context.OutgoingTransportMessage, sendOptions)
                    .ConfigureAwait(false);
            }

            var publishOptions = context.Options as PublishOptions;
            if (publishOptions != null)
            {
                await this.publisher.PublishAsync(context.OutgoingTransportMessage, publishOptions)
                    .ConfigureAwait(false);
            }

            await next()
                .ConfigureAwait(false);
        }
    }
}