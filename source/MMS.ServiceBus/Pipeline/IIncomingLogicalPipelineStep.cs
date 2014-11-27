namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    using System;
    using System.Threading.Tasks;

    public interface IIncomingLogicalPipelineStep
    {
        Task Invoke(IncomingLogicalContext context, IBus bus, Func<Task> next);
    }
}