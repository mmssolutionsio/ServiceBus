using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public class HandlerRegistry
    {
        // TODO: Strong typing?
        public virtual IReadOnlyCollection<object> GetHandlers(Type messageType)
        {
            return new ReadOnlyCollection<object>(new List<object>());
        }

        public virtual async Task InvokeHandle(object handler, object message, IBus bus)
        {
            dynamic h = handler;
            dynamic m = message;
            await h.Handle(m, bus);
        }
    }
}