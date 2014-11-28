using System;
using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public interface IOutgoingTransportPipelineStep
    {
        Task Invoke(OutgoingTransportContext context, Func<Task> next);
    }
}