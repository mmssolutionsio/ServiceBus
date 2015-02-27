//-------------------------------------------------------------------------------
// <copyright file="OutgoingLogicalContext.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    public class OutgoingLogicalContext : Context
    {
        public OutgoingLogicalContext(LogicalMessage message, DeliveryOptions options, EndpointConfiguration.ReadOnly configuration, ITransactionEnlistment enlistment)
            : base(configuration, enlistment)
        {
            this.Set(message);
            this.Set(options);
        }

        public LogicalMessage LogicalMessage
        {
            get
            {
                return this.Get<LogicalMessage>();
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