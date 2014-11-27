namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    using System;
    using System.Threading.Tasks;

    public interface IOutgoingLogicalPipelineStep
    {
        Task Invoke(OutgoingLogicalContext context, Func<Task> next);
    }
}