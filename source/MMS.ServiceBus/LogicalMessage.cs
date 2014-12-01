//-------------------------------------------------------------------------------
// <copyright file="LogicalMessage.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;

    public class LogicalMessage
    {
        private readonly IDictionary<string, string> headers;

        private readonly object message;
        private readonly Type messageType;

        public LogicalMessage(object message, IDictionary<string, string> headers)
            : this(message.GetType(), message, headers)
        {
        }

        public LogicalMessage(Type messageType, object message, IDictionary<string, string> headers)
        {
            this.messageType = messageType;
            this.message = message;
            this.headers = headers;
        }

        public Type MessageType
        {
            get
            {
                return this.messageType;
            }
        }

        public object Instance
        {
            get { return this.message; }
        }

        public IDictionary<string, string> Headers
        {
            get { return this.headers; }
        }
    }
}