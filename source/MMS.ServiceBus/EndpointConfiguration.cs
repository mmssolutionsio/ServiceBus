//-------------------------------------------------------------------------------
// <copyright file="EndpointConfiguration.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using JetBrains.Annotations;

    public class EndpointConfiguration
    {
        public EndpointConfiguration()
        {
            this.Concurrency(Environment.ProcessorCount);
        }

        public Queue EndpointQueue { get; private set; }

        internal int MaxConcurrency { get; private set; }

        internal int PrefetchCount { get; private set; }

        internal bool IsTransactional { get; private set; }

        public EndpointConfiguration Endpoint([NotNull] string endpointName)
        {
            this.EndpointQueue = Queue.Create(endpointName);
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

        internal ReadOnly Validate()
        {
            if (this.EndpointQueue == null)
            {
                throw new InvalidOperationException("The endpoint name must be set by calling configuration.Endpoint(\"EndpointName\").");
            }

            return new ReadOnly(this);
        }

        public class ReadOnly
        {
            protected internal ReadOnly(EndpointConfiguration configuration)
            {
                this.EndpointQueue = configuration.EndpointQueue;
                this.MaxConcurrency = configuration.MaxConcurrency;
                this.PrefetchCount = configuration.PrefetchCount;
                this.IsTransactional = configuration.IsTransactional;
            }

            public bool IsTransactional { get; private set; }

            public Queue EndpointQueue { get; private set; }

            public int MaxConcurrency { get; private set; }

            public int PrefetchCount { get; private set; }
        }
    }
}