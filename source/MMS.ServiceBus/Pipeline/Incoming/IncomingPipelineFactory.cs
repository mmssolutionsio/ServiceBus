//-------------------------------------------------------------------------------
// <copyright file="IncomingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System.Threading.Tasks;

    public class IncomingPipelineFactory : IIncomingPipelineFactory
    {
        private readonly IHandlerRegistry registry;

        public IncomingPipelineFactory(IHandlerRegistry registry)
        {
            this.registry = registry;
        }

        public Task WarmupAsync()
        {
            return Task.FromResult(0);
        }

        public IncomingPipeline Create()
        {
            var pipeline = new IncomingPipeline();

            pipeline.Transport
                .Register(new DeadLetterMessagesWhichCantBeDeserializedStep())
                .Register(new DeserializeTransportMessageStep(new DataContractMessageSerializer()));

            pipeline.Logical
                .Register(new LoadMessageHandlersStep(this.registry))
                .Register(new InvokeHandlerStep());

            return pipeline;
        }

        public Task CooldownAsync()
        {
            return Task.FromResult(0);
        }
    }
}