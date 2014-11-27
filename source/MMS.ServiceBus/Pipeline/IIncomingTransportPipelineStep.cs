namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    using System;
    using System.Threading.Tasks;

    public interface IIncomingTransportPipelineStep
    {
        Task Invoke(IncomingTransportContext context, IBus bus, Func<Task> next);
    }
}