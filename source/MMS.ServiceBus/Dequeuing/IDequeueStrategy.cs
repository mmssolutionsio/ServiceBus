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
        Task StartAsync(EndpointConfiguration.ReadOnly configuration, Func<TransportMessage, Task> onMessage);

        Task StopAsync();
    }
}