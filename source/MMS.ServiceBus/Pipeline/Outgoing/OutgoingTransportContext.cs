//-------------------------------------------------------------------------------
// <copyright file="OutgoingTransportContext.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    public class OutgoingTransportContext : Context
    {
        private const string OutgoingTransportMessageKey = "OutgoingTransportMessage";
        private const string IncomingTransportMessageKey = "IncomingTransportMessage";

        public OutgoingTransportContext(LogicalMessage message, TransportMessage outgoingTransportMessage, DeliveryOptions options, EndpointConfiguration.ReadOnly configuration, ITransactionEnlistment enlistment, TransportMessage incomingTransportMessage = null)
            : base(configuration, enlistment)
        {
            this.Set(message);
            this.Set(OutgoingTransportMessageKey, outgoingTransportMessage);
            this.Set(IncomingTransportMessageKey, incomingTransportMessage);
            this.Set(options);
        }

        public LogicalMessage LogicalMessage
        {
            get
            {
                return this.Get<LogicalMessage>();
            }
        }

        public TransportMessage OutgoingTransportMessage
        {
            get
            {
                return this.Get<TransportMessage>(OutgoingTransportMessageKey);
            }
        }

        public TransportMessage IncomingTransportMessage
        {
            get
            {
                return this.Get<TransportMessage>(IncomingTransportMessageKey);
            }
        }

        public DeliveryOptions Options
        {
            get
            {
                return this.Get<DeliveryOptions>();
            }
        }
    }
}