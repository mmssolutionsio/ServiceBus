using System;
using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public interface IIncomingLogicalPipelineStep
    {
        Task Invoke(IncomingLogicalContext context, IBus bus, Func<Task> next);
    }
}