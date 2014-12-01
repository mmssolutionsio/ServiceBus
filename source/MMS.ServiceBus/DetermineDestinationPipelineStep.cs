//-------------------------------------------------------------------------------
// <copyright file="DetermineDestinationPipelineStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Pipeline;

    public class DetermineDestinationPipelineStep : IOutgoingTransportPipelineStep
    {
        private readonly MessageRouter router;

        public DetermineDestinationPipelineStep(MessageRouter router)
        {
            this.router = router;
        }

        public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            var sendOptions = context.Options as SendOptions;
            if (sendOptions != null)
            {
                sendOptions.Destination = this.GetDestinationForSend(context.LogicalMessage.Instance);
            }

            var publishOptions = context.Options as PublishOptions;
            if (publishOptions != null)
            {
                publishOptions.Destination = this.GetDestinationForPublish(context.LogicalMessage.Instance);
            }

            await next();
        }

        private Queue GetDestinationForSend(object message)
        {
            IReadOnlyCollection<Address> destinations = this.router.GetDestinationFor(message.GetType());

            if (destinations.Count > 1)
            {
                throw new InvalidOperationException("Sends can only have one target address.");
            }

            return destinations.OfType<Queue>().Single();
        }

        private Topic GetDestinationForPublish(object message)
        {
            IReadOnlyCollection<Address> destinations = this.router.GetDestinationFor(message.GetType());

            if (destinations.Count > 1)
            {
                throw new InvalidOperationException("Publish can only have one target address.");
            }

            return destinations.OfType<Topic>().Single();
        }
    }
}