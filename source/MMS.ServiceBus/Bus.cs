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

        public Task StartAsync()
        {
            this.configuration.Validate();

            return this.strategy.StartAsync(this.OnMessageAsync);
        }

        public Task SendLocal(object message)
        {
            return this.SendLocal(message, incoming: null);
        }

        public Task Send(object message, SendOptions options = null)
        {
            return this.Send(message, options, incoming: null);
        }

        public Task Publish(object message, PublishOptions options = null)
        {
            return this.Publish(message, options, incoming: null);
        }

        public Task Reply(object message)
        {
            throw new InvalidOperationException("You can only reply in a handler context!");
        }

        public void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            throw new InvalidOperationException("You can only abort the pipeline in a handler context!");
        }

        public Task StopAsync()
        {
            return this.strategy.StopAsync();
        }

        private Task SendLocal(object message, TransportMessage incoming)
        {
            return this.Send(message, new SendOptions { Destination = this.configuration.EndpointQueue }, incoming);
        }

        private Task Send(object message, SendOptions options, TransportMessage incoming)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "You cannot send null");
            }

            var sendOptions = options ?? new SendOptions();
            LogicalMessage msg = this.factory.Create(message, sendOptions.Headers);

            return this.SendMessage(msg, sendOptions, incoming);
        }

        private Task Publish(object message, PublishOptions options, TransportMessage incoming)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "You cannot publish null");
            }

            var publishOptions = options ?? new PublishOptions();
            LogicalMessage msg = this.factory.Create(message, publishOptions.Headers);
            publishOptions.EventType = msg.MessageType;

            return this.SendMessage(msg, publishOptions, incoming);
        }

        private Task SendMessage(LogicalMessage outgoingLogicalMessage, DeliveryOptions options, TransportMessage incoming)
        {
            if (options.ReplyToAddress == null)
            {
                options.ReplyToAddress = this.configuration.EndpointQueue;
            }

            OutgoingPipeline outgoingPipeline = this.outgoingPipelineFactory.Create();
            return outgoingPipeline.Invoke(outgoingLogicalMessage, options, incoming);
        }

        private Task OnMessageAsync(TransportMessage message)
        {
            IncomingPipeline incomingPipeline = this.incomingPipelineFactory.Create();
            return incomingPipeline.Invoke(new IncomingBusDecorator(this, incomingPipeline, message), message);
        }

        private class IncomingBusDecorator : IBus
        {
            private readonly Bus bus;

            private readonly IncomingPipeline incomingPipeline;
            private readonly TransportMessage incoming;

            public IncomingBusDecorator(Bus bus, IncomingPipeline incomingPipeline, TransportMessage incoming)
            {
                this.incoming = incoming;
                this.incomingPipeline = incomingPipeline;
                this.bus = bus;
            }

            public Task SendLocal(object message)
            {
                return this.bus.SendLocal(message, this.incoming);
            }

            public Task Send(object message, SendOptions options = null)
            {
                return this.bus.Send(message, options, this.incoming);
            }

            public Task Publish(object message, PublishOptions options = null)
            {
                return this.bus.Publish(message, options, this.incoming);
            }

            public Task Reply(object message)
            {
                ReplyOptions replyOptions = CreateReplyOptions(this.incoming);
                return this.bus.Send(message, replyOptions, this.incoming);
            }

            public void DoNotContinueDispatchingCurrentMessageToHandlers()
            {
                this.incomingPipeline.DoNotInvokeAnyMoreHandlers();
            }

            private static ReplyOptions CreateReplyOptions(TransportMessage incoming)
            {
                Queue destination = incoming.ReplyTo;

                string correlationId = !string.IsNullOrEmpty(incoming.CorrelationId)
                    ? incoming.CorrelationId
                    : incoming.Id;

                return new ReplyOptions(destination, correlationId);
            }
        }
    }
}