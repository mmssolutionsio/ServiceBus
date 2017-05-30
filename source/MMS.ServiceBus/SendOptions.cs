//-------------------------------------------------------------------------------
// <copyright file="SendOptions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;

    public class SendOptions : DeliveryOptions
    {
        public Queue Queue { get; set; }

        public string CorrelationId { get; set; }

        public DateTime ScheduledEnqueueTimeUtc { get; set; }

        public int DelayedDeliveryCount { get; set; }
    }
}