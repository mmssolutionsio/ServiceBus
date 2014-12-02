//-------------------------------------------------------------------------------
// <copyright file="Bus.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;
    using Pipeline.Incoming;
    using Pipeline.Outgoing;

    public class Bus : IBus
    {
        private readonly EndpointConfiguration configuration;

        private readonly DequeueStrategy strategy;

        private readonly LogicalMessageFactory factory;

        private readonly IOutgoingPipelineFactory outgoingPipelineFactory;

        private readonly IIncomingPipelineFactory incomingPipelineFactory;

        public Bus(
            EndpointConfiguration configuration, 
            DequeueStrategy strategy, 
            LogicalMessageFactory factory,
            IOutgoingPipelineFactory outgoingPipelineFactory, 
            IIncomingPipelineFactory incomingPipelineFactory)
        {
            this.incomingPipelineFactory = incomingPipelineFactory;
            this.outgoingPipelineFactory = outgoingPipelineFactory;

            this.factory = factory;
            this.configuration = configuration;
            this.strategy = strategy;
        }

        public async Task StartAsync()
        {
            this.configuration.Validate();

            await this.strategy.StartAsync(this.OnMessageAsync);
        }

        public Task SendLocal(object message)
        {
            return this.Send(message, new SendOptions { Destination = this.configuration.EndpointQueue });
        }

        public Task Send(object message, SendOptions options = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "You cannot send null");
            }

            var sendOptions = options ?? new SendOptions();
            LogicalMessage msg = this.factory.Create(message, sendOptions.Headers);

            return this.SendMessage(msg, sendOptions);
        }

        public Task Publish(object message, PublishOptions options = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "You cannot publish null");
            }

            var publishOptions = options ?? new PublishOptions();
            LogicalMessage msg = this.factory.Create(message, publishOptions.Headers);
            publishOptions.EventType = msg.MessageType;

            return this.SendMessage(msg, publishOptions);
        }

        public void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            // This has only an effect when called in an incoming pipeline.
        }

        public async Task StopAsync()
        {
            await this.strategy.StopAsync();
        }

        private async Task SendMessage(LogicalMessage message, DeliveryOptions options)
        {
            if (options.ReplyToAddress == null)
            {
                options.ReplyToAddress = this.configuration.EndpointQueue;
            }

            OutgoingPipeline outgoingPipeline = this.outgoingPipelineFactory.Create();
            await outgoingPipeline.Invoke(message, options);
        }

        private Task OnMessageAsync(TransportMessage message)
        {
            IncomingPipeline incomingPipeline = this.incomingPipelineFactory.Create();
            return incomingPipeline.Invoke(new IncomingBusDecorator(this, incomingPipeline), message);
        }

        private class IncomingBusDecorator : IBus
        {
            private readonly IBus bus;

            private readonly IncomingPipeline incomingPipeline;

            public IncomingBusDecorator(IBus bus, IncomingPipeline incomingPipeline)
            {
                this.incomingPipeline = incomingPipeline;
                this.bus = bus;
            }

            public Task SendLocal(object message)
            {
                return this.bus.SendLocal(message);
            }

            public Task Send(object message, SendOptions options = null)
            {
                return this.bus.Send(message, options);
            }

            public Task Publish(object message, PublishOptions options = null)
            {
                return this.bus.Publish(message, options);
            }

            public void DoNotContinueDispatchingCurrentMessageToHandlers()
            {
                this.incomingPipeline.DoNotInvokeAnyMoreHandlers();
            }
        }
    }
}