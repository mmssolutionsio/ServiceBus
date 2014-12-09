//-------------------------------------------------------------------------------
// <copyright file="DetermineDestinationStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DetermineDestinationStep : IOutgoingTransportStep
    {
        private readonly MessageRouter router;

        public DetermineDestinationStep(MessageRouter router)
        {
            this.router = router;
        }

        public Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            var sendOptions = context.Options as SendOptions;
            if (ShouldDetermineSendDestination(sendOptions))
            {
// ReSharper disable PossibleNullReferenceException
                sendOptions.Queue = this.GetDestinationForSend(context.LogicalMessage.Instance);
// ReSharper restore PossibleNullReferenceException
            }

            var publishOptions = context.Options as PublishOptions;
            if (ShouldDeterminePublishDestination(publishOptions))
            {
// ReSharper disable PossibleNullReferenceException
                publishOptions.Topic = this.GetDestinationForPublish(context.LogicalMessage.Instance);
// ReSharper restore PossibleNullReferenceException
            }

            return next();
        }

        private static bool ShouldDeterminePublishDestination(PublishOptions publishOptions)
        {
            return publishOptions != null && publishOptions.Topic == null;
        }

        private static bool ShouldDetermineSendDestination(SendOptions sendOptions)
        {
            return sendOptions != null && sendOptions.Queue == null;
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