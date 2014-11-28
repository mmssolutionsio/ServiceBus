namespace MMS.ServiceBus
{
    using Microsoft.ServiceBus.Messaging;
    using Pipeline;

    public class OutgoingPipelineFactory
    {
        private readonly MessagingFactory factory;

        public OutgoingPipelineFactory()
        {
        }

        public OutgoingPipelineFactory(MessagingFactory factory)
        {
            this.factory = factory;
        }

        public virtual OutgoingPipeline Create()
        {
            var pipeline = new OutgoingPipeline();

            return pipeline
                .RegisterStep(new CreateTransportMessagePipelineStep())
                .RegisterStep(new SerializeMessagePipelineStep(new DataContractMessageSerializer()))
                .RegisterStep(new DispatchToTransportPipelineStep(new MessageSender(this.factory)));
        }
    }
}