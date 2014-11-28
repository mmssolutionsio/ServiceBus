using System;
using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public class MessageHandler
    {
        public object Instance { get; set; }

        public Func<object, object, Task> Invocation { get; set; }
    }
}