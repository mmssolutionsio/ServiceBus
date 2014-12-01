//-------------------------------------------------------------------------------
// <copyright file="IncomingLogicalContext.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    public class IncomingLogicalContext : Context
    {
        private const string HandlerInvocationAbortedKey = "HandlerInvocationAborted";

        public IncomingLogicalContext(LogicalMessage logicalMessage, TransportMessage message)
        {
            this.Set(logicalMessage);
            this.Set(message);
            this.Set(HandlerInvocationAbortedKey, false);
        }

        public LogicalMessage LogicalMessage
        {
            get
            {
                return this.Get<LogicalMessage>();
            }
        }

        public TransportMessage TransportMessage
        {
            get
            {
                return this.Get<TransportMessage>();
            }
        }

        public MessageHandler Handler
        {
            get
            {
                return this.Get<MessageHandler>();
            }

            set
            {
                this.Set(value);
            }
        }

        public bool HandlerInvocationAborted
        {
            get
            {
                return this.Get<bool>(HandlerInvocationAbortedKey);
            }

            set
            {
                this.Set<bool>(HandlerInvocationAbortedKey, value);
            }
        }
    }
}