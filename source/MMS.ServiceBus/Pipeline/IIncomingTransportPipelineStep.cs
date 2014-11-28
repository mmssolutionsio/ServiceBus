using System;
using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public interface IIncomingTransportPipelineStep
    {
        Task Invoke(IncomingTransportContext context, IBus bus, Func<Task> next);
    }
}