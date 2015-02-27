//-------------------------------------------------------------------------------
// <copyright file="IDequeueStrategy.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Dequeuing
{
    using System;
    using System.Threading.Tasks;

    public interface IDequeueStrategy
    {
        Task StartAsync(EndpointConfiguration.ReadOnly configuration, ITransactionalBusProvider transactionalBusProvider, Func<TransportMessage, ITransaction, Task> onMessage);

        Task StopAsync();
    }
}