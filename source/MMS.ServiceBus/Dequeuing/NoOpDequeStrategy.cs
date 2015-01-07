//-------------------------------------------------------------------------------
// <copyright file="NoOpDequeStrategy.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Dequeuing
{
    using System;
    using System.Threading.Tasks;

    internal class NoOpDequeStrategy : IDequeueStrategy
    {
        public Task StartAsync(EndpointConfiguration.ReadOnly configuration, ITransactionalBusProvider transactionalBusProvider, Func<TransportMessage, ITransaction, Task> onMessage)
        {
            return Task.FromResult(0);
        }

        public Task StopAsync()
        {
            return Task.FromResult(0);
        }
    }
}