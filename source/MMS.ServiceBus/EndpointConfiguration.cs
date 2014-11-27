namespace MMS.Common.ServiceBusWrapper
{
    using System.CodeDom;
    using System.Collections.Concurrent;
    using System.Runtime.ExceptionServices;
    using System.Threading;

    public class EndpointConfiguration
    {
        public EndpointConfiguration Endpoint(string endpointName)
        {
            this.EndpointQueue = new Queue(endpointName);
            return this;
        }

        public EndpointConfiguration Concurrency(int maxConcurrency)
        {
            this.MaxConcurrency = maxConcurrency;
            this.PrefetchCount = maxConcurrency;
            return this;
        }

        public EndpointConfiguration Transactional()
        {
            this.IsTransactional = true;
            return this;
        }

        internal bool IsTransactional { get; private set; }

        internal int MaxConcurrency { get; private set; }

        internal int PrefetchCount { get; set; }

        internal Queue EndpointQueue { get; private set; }
    }

    public class SendOptions : DeliveryOptions
    {
        public SendOptions(Address address)
        {
            this.Destination = address;
        }

        public Address Destination { get; private set; }
    }
}