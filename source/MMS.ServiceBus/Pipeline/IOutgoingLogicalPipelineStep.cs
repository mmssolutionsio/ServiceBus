using System;
using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public interface IOutgoingLogicalPipelineStep
    {
        Task Invoke(OutgoingLogicalContext context, Func<Task> next);
    }
}