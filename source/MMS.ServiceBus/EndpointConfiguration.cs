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
            this.ImmediateRetryCount = 10;
        }

        public Queue EndpointQueue { get; private set; }

        internal int MaxConcurrency { get; private set; }

        internal int PrefetchCount { get; private set; }

        internal int ImmediateRetryCount { get; private set; }

        internal int DelayedRetryCount { get; private set; }

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

        /// <summary>
        /// Set the maximum number of immediate retries.
        /// </summary>
        /// <param name="count">maximum number of immediate retries</param>
        /// <returns>EndpointConfiguration with maximum number of immediate retries</returns>
        public EndpointConfiguration MaximumImmediateRetryCount(int count)
        {
            this.ImmediateRetryCount = count;
            return this;
        }

        /// <summary>
        /// Set the maximum number of delayed retries. 
        /// If the delivery of the message fails again, it will be immediatly retried as configured with MaximumImmediateRetryCount.
        /// If these immediate retries fail also, the message will be delayed again until this counter is reached.
        /// https://docs.particular.net/nservicebus/recoverability/
        /// </summary>
        /// <param name="count">maximum number of delayed retries</param>
        /// <returns>EndpointConfiguration with maximum number of delayed retries</returns>
        public EndpointConfiguration MaximumDelayedRetryCount(int count)
        {
            this.DelayedRetryCount = count;
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
                this.ImmediateRetryCount = configuration.ImmediateRetryCount;
                this.DelayedRetryCount = configuration.DelayedRetryCount;
            }

            public Queue EndpointQueue { get; private set; }

            public int MaxConcurrency { get; private set; }

            public int PrefetchCount { get; private set; }

            public int ImmediateRetryCount { get; private set; }

            public int DelayedRetryCount { get; private set; }
        }
    }
}