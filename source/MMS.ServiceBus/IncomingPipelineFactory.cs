//-------------------------------------------------------------------------------
// <copyright file="IncomingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using Pipeline;

    public class IncomingPipelineFactory : IIncomingPipelineFactory
    {
        private readonly HandlerRegistry registry;

        public IncomingPipelineFactory(HandlerRegistry registry)
        {
            this.registry = registry;
        }

        public virtual IncomingPipeline Create()
        {
            var pipeline = new IncomingPipeline();
            return pipeline
                .RegisterStep(new DeserializeTransportMessagePipelineStep(new DataContractMessageSerializer(), new LogicalMessageFactory()))
                .RegisterStep(new LoadMessageHandlers(this.registry))
                .RegisterStep(new InvokeHandlers());
        }
    }
}