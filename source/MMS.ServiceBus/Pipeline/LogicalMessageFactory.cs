//-------------------------------------------------------------------------------
// <copyright file="LogicalMessageFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Collections.Generic;

    internal class LogicalMessageFactory
    {
        public LogicalMessage Create(object message, IDictionary<string, string> headers)
        {
            return this.Create(message.GetType(), message, headers);
        }

        public LogicalMessage Create(Type messageType, object message, IDictionary<string, string> headers)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (messageType == null)
            {
                throw new ArgumentNullException("messageType");
            }

            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            return new LogicalMessage(messageType, message, headers);
        }
    }
}