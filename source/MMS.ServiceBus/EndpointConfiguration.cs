//-------------------------------------------------------------------------------
// <copyright file="EndpointConfiguration.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    public class EndpointConfiguration
    {
        public Queue EndpointQueue { get; private set; }

        internal bool IsTransactional { get; private set; }

        internal int MaxConcurrency { get; private set; }

        internal int PrefetchCount { get; private set; }

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
    }
}