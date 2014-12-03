//-------------------------------------------------------------------------------
// <copyright file="IncomingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    public class IncomingPipelineFactory : IIncomingPipelineFactory
    {
        private readonly HandlerRegistry registry;

        public IncomingPipelineFactory(HandlerRegistry registry)
        {
            this.registry = registry;
        }

        public IncomingPipeline Create()
        {
            var pipeline = new IncomingPipeline();
            return pipeline
                .RegisterStep(new DeserializeTransportMessageStep(new DataContractMessageSerializer(), new LogicalMessageFactory()))
                .RegisterStep(new LoadMessageHandlersStep(this.registry))
                .RegisterStep(new InvokeHandlerStep());
        }
    }
}