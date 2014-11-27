namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    using System;
    using System.Threading.Tasks;

    public interface IOutgoingTransportPipelineStep
    {
        Task Invoke(OutgoingTransportContext context, Func<Task> next);
    }
}