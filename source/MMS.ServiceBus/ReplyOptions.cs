//-------------------------------------------------------------------------------
// <copyright file="ReplyOptions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------d

namespace MMS.ServiceBus
{
    using System;

    public class ReplyOptions : SendOptions
    {
        public ReplyOptions(Queue destination, string correlationId)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination", "The destination must always be specified in a reply scenario");
            }

            if (string.IsNullOrEmpty(correlationId))
            {
                throw new ArgumentNullException("correlationId", "The correlation id must always be specified in a reply scenario");
            }

            this.CorrelationId = correlationId;
            this.Queue = destination;
        }
    }
}