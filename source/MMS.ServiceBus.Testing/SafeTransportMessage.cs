namespace MMS.ServiceBus.Testing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public class SafeTransportMessage : TransportMessage
    {
        private int deliveryCount = 0;

        public SafeTransportMessage(BrokeredMessage message) : base(message)
        {
        }

        public override int DeliveryCount
        {
            // A bit evil but hey!
            get { return this.deliveryCount++; }
        }

        protected override Task DeadLetterAsyncInternal(IDictionary<string, object> deadLetterHeaders)
        {
            this.DeadLetterHeaders = deadLetterHeaders;

            return Task.FromResult(0);
        }

        public IDictionary<string, object> DeadLetterHeaders { get; private set; }
    }
}