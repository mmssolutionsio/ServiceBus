//-------------------------------------------------------------------------------
// <copyright file="IncomingLogicalContext.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    public class IncomingLogicalContext : Context
    {
        private const string HandlerInvocationAbortPendingKey = "HandlerInvocationAbortPending";

        public IncomingLogicalContext(LogicalMessage logicalMessage, TransportMessage message, EndpointConfiguration.ReadOnly configuration)
            : base(configuration, null)
        {
            this.Set(logicalMessage);
            this.Set(message);
            this.Set<MessageHandler>(null, ShouldBeSnapshotted.Yes);
            this.Set(HandlerInvocationAbortPendingKey, false);
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
                this.Set(value, ShouldBeSnapshotted.Yes);
            }
        }

        public bool HandlerInvocationAbortPending
        {
            get
            {
                return this.Get<bool>(HandlerInvocationAbortPendingKey);
            }
        }

        public void AbortHandlerInvocation()
        {
            this.Set(HandlerInvocationAbortPendingKey, true);
        }
    }
}