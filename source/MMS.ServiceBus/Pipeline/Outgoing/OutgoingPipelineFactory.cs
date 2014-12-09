//-------------------------------------------------------------------------------
// <copyright file="OutgoingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public class OutgoingPipelineFactory : IOutgoingPipelineFactory
    {
        private readonly MessagingFactory factory;
        private readonly MessageRouter router;
        private MessageSender sender;
        private MessagePublisher publisher;

        public OutgoingPipelineFactory(MessagingFactory factory, MessageRouter router)
        {
            this.router = router;
            this.factory = factory;
        }

        public Task WarmupAsync()
        {
            this.sender = new MessageSender(this.factory);
            this.publisher = new MessagePublisher(this.factory);

            return Task.FromResult(0);
        }

        public OutgoingPipeline Create()
        {
            var pipeline = new OutgoingPipeline();

            pipeline.Logical
                .Register(new CreateTransportMessageStep());

            pipeline.Transport
                .Register(new SerializeMessageStep(new DataContractMessageSerializer()))
                .Register(new DetermineDestinationStep(this.router))
                .Register(new DispatchToTransportStep(this.sender, this.publisher));

            return pipeline;
        }

        public async Task CooldownAsync()
        {
            await this.sender.CloseAsync();
            await this.publisher.CloseAsync();
        }
    }
}