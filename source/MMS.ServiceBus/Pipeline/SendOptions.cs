//-------------------------------------------------------------------------------
// <copyright file="SendOptions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    public class SendOptions : DeliveryOptions
    {
        public Queue Destination { get; set; }

        public string CorrelationId { get; set; }
    }
}