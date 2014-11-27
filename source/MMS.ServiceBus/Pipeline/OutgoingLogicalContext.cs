namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    public class OutgoingLogicalContext : Context
    {
        public OutgoingLogicalContext(LogicalMessage message, DeliveryOptions options)
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