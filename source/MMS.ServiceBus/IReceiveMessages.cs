//-------------------------------------------------------------------------------
// <copyright file="IReceiveMessages.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Threading.Tasks;

    public interface IReceiveMessages
    {
        Task<AsyncClosable> StartAsync(EndpointConfiguration configuration, Func<TransportMessage, Task> onMessage);
    }
}