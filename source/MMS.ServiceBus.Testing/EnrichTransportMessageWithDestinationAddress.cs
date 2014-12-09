//-------------------------------------------------------------------------------
// <copyright file="EnrichTransportMessageWithDestinationAddress.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

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
                context.OutgoingTransportMessage.Headers[AcceptanceTestHeaders.Destination] = sendOptions.Queue.ToString();
            }

            var publishOptions = context.Options as PublishOptions;
            if (publishOptions != null)
            {
                context.OutgoingTransportMessage.Headers[AcceptanceTestHeaders.Destination] = publishOptions.Topic.ToString();
            }

            await next();
        }
    }
}