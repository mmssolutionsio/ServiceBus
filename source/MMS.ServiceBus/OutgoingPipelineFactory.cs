//-------------------------------------------------------------------------------
// <copyright file="OutgoingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using Microsoft.ServiceBus.Messaging;
    using Pipeline;
    using MessageSender = Pipeline.MessageSender;

    public class OutgoingPipelineFactory : IOutgoingPipelineFactory
    {
        private readonly MessagingFactory factory;
        private readonly MessageRouter router;

        public OutgoingPipelineFactory(MessagingFactory factory, MessageRouter router)
        {
            this.router = router;
            this.factory = factory;
        }

        public virtual OutgoingPipeline Create()
        {
            var pipeline = new OutgoingPipeline();

            return pipeline
                .RegisterStep(new CreateTransportMessagePipelineStep())
                .RegisterStep(new SerializeMessagePipelineStep(new DataContractMessageSerializer()))
                .RegisterStep(new DetermineDestinationPipelineStep(this.router))
                .RegisterStep(new DispatchToTransportPipelineStep(new MessageSender(this.factory), new MessagePublisher(this.factory)));
        }
    }
}