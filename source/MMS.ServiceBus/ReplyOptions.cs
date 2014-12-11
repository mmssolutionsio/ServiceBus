//-------------------------------------------------------------------------------
// <copyright file="ReplyOptions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------d

namespace MMS.ServiceBus
{
    public class ReplyOptions : SendOptions
    {
        public ReplyOptions()
        {
        }

        public ReplyOptions(Queue destination, string correlationId)
        {
            this.CorrelationId = correlationId;
            this.Queue = destination;
        }
    }
}