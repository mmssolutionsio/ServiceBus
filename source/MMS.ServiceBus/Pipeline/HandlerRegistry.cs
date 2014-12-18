//-------------------------------------------------------------------------------
// <copyright file="HandlerRegistry.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class HandlerRegistry : IHandlerRegistry
    {
        public virtual IReadOnlyCollection<object> GetHandlers(Type messageType)
        {
            return this.ConsumeAll();
        }

        public virtual Task InvokeHandle(object handler, object message, IBusForHandler bus)
        {
            dynamic h = handler;
            dynamic m = message;
            return h.Handle(m, bus);
        }
    }
}