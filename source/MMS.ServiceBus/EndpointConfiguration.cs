//-------------------------------------------------------------------------------
// <copyright file="EndpointConfiguration.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;

    public class EndpointConfiguration
    {
        public EndpointConfiguration()
        {
            this.Concurrency(Environment.ProcessorCount);
        }

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

        internal void Validate()
        {
            if (this.EndpointQueue == null)
            {
                throw new InvalidOperationException("The endpoint name must be set by calling configuration.Endpoint(\"EndpointName\").");
            }
        }
    }
}