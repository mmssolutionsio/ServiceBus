//-------------------------------------------------------------------------------
// <copyright file="HandlerRegistry.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    public class HandlerRegistry
    {
        // TODO: Strong typing?
        public virtual IReadOnlyCollection<object> GetHandlers(Type messageType)
        {
            return new ReadOnlyCollection<object>(new List<object>());
        }

        public virtual Task InvokeHandle(object handler, object message, IBus bus)
        {
            dynamic h = handler;
            dynamic m = message;
            return h.Handle(m, bus);
        }
    }
}