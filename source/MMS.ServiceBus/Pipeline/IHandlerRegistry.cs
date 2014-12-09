//-------------------------------------------------------------------------------
// <copyright file="IHandlerRegistry.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IHandlerRegistry
    {
        IReadOnlyCollection<object> GetHandlers(Type messageType);

        Task InvokeHandle(object handler, object message, IBusForHandler bus);
    }
}