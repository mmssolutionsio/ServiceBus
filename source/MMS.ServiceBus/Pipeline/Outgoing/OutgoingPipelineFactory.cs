//-------------------------------------------------------------------------------
// <copyright file="OutgoingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using Microsoft.ServiceBus.Messaging;

    public class OutgoingPipelineFactory : IOutgoingPipelineFactory
    {
        private readonly MessagingFactory factory;
        private readonly MessageRouter router;

        public OutgoingPipelineFactory(MessagingFactory factory, MessageRouter router)
        {
            this.router = router;
            this.factory = factory;
        }

        public OutgoingPipeline Create()
        {
            var pipeline = new OutgoingPipeline();

            pipeline.Logical
                .Register(new CreateTransportMessageStep());

            pipeline.Transport
                .Register(new SerializeMessageStep(new DataContractMessageSerializer()))
                .Register(new DetermineDestinationStep(this.router))
                .Register(new DispatchToTransportStep(new MessageSender(this.factory), new MessagePublisher(this.factory)));

            return pipeline;
        }
    }
}