namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    using System;
    using System.Threading.Tasks;

    public class MessageHandler
    {
        public object Instance { get; set; }

        public Func<object, object, Task> Invocation { get; set; }
    }
}