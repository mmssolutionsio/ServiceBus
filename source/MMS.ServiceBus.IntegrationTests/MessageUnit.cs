//-------------------------------------------------------------------------------
// <copyright file="MessageUnit.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Pipeline;
    using Pipeline.Incoming;
    using Pipeline.Outgoing;

    public class MessageUnit : IBus
    {
        protected readonly EndpointConfiguration configuration;

        private Bus unit;

        protected HandlerRegistry registry;

        protected MessageRouter router;

        protected MessagingFactory factory;

        public MessageUnit(EndpointConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public MessageUnit Use(HandlerRegistry registry)
        {
            this.registry = registry;
            return this;
        }

        public MessageUnit Use(MessageRouter router)
        {
            this.router = router;
            return this;
        }

        public MessageUnit Use(MessagingFactory factory)
        {
            this.factory = factory;
            return this;
        }

        public async Task StartAsync()
        {
            IOutgoingPipelineFactory outgoingFactory = this.CreateOutgoingPipelineFactory();
            IIncomingPipelineFactory incomingFactory = this.CreateIncomingPipelineFactory();

            this.unit = this.CreateBus(outgoingFactory, incomingFactory);
            this.SetupNecessaryInfrastructure();
            await this.unit.StartAsync();
        }

        public async Task StopAsync()
        {
            await this.unit.StopAsync();
        }

        public Task Send(object message, SendOptions options = null)
        {
            return this.unit.Send(message, options);
        }

        public Task Publish(object message, PublishOptions options = null)
        {
            return this.unit.Publish(message, options);
        }

        public void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            this.unit.DoNotContinueDispatchingCurrentMessageToHandlers();
        }

        protected virtual IIncomingPipelineFactory CreateIncomingPipelineFactory()
        {
            return new IncomingPipelineFactory(this.registry);
        }

        protected virtual IOutgoingPipelineFactory CreateOutgoingPipelineFactory()
        {
            return new OutgoingPipelineFactory(this.factory, this.router);
        }

        protected virtual Bus CreateBus(IOutgoingPipelineFactory outgoingPipelineFactory, IIncomingPipelineFactory incomingPipelineFactory)
        {
            return new Bus(this.configuration, new DequeueStrategy(this.configuration, new MessageReceiverReceiver(this.factory)), new LogicalMessageFactory(), outgoingPipelineFactory, incomingPipelineFactory);
        }

        protected virtual void SetupNecessaryInfrastructure()
        {
            var manager = NamespaceManager.Create();
            if (!manager.QueueExists(this.configuration.EndpointQueue))
            {
                manager.CreateQueue(this.configuration.EndpointQueue);
            }
        }
    }
}