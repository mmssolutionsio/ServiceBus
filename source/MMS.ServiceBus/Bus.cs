//-------------------------------------------------------------------------------
// <copyright file="Bus.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dequeuing;
    using Pipeline;
    using Pipeline.Incoming;
    using Pipeline.Outgoing;

    public class Bus : IBus
    {
        private readonly EndpointConfiguration configuration;

        private readonly IDequeueStrategy strategy;

        private readonly LogicalMessageFactory factory;

        private readonly IOutgoingPipelineFactory outgoingPipelineFactory;

        private readonly IIncomingPipelineFactory incomingPipelineFactory;

        private EndpointConfiguration.ReadOnly readOnlyConfiguration;

        public Bus(
            EndpointConfiguration configuration,
            IDequeueStrategy strategy,
            IOutgoingPipelineFactory outgoingPipelineFactory,
            IIncomingPipelineFactory incomingPipelineFactory)
        {
            this.incomingPipelineFactory = incomingPipelineFactory;
            this.outgoingPipelineFactory = outgoingPipelineFactory;

            this.factory = new LogicalMessageFactory();
            this.configuration = configuration;
            this.strategy = strategy;
        }

        public async Task StartAsync()
        {
            this.readOnlyConfiguration = this.configuration.Validate();

            await this.outgoingPipelineFactory.WarmupAsync();
            await this.incomingPipelineFactory.WarmupAsync();
            await this.strategy.StartAsync(this.readOnlyConfiguration, this.OnMessageAsync);
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

        public async Task StopAsync()
        {
            await this.strategy.StopAsync();
            await this.incomingPipelineFactory.CooldownAsync();
            await this.outgoingPipelineFactory.CooldownAsync();
        }

        private Task SendLocal(object message, TransportMessage incoming)
        {
            return this.Send(message, new SendOptions { Queue = this.readOnlyConfiguration.EndpointQueue }, incoming);
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
            return outgoingPipeline.Invoke(outgoingLogicalMessage, options, this.readOnlyConfiguration, incoming);
        }

        private Task OnMessageAsync(TransportMessage message)
        {
            IncomingPipeline incomingPipeline = this.incomingPipelineFactory.Create();
            return incomingPipeline.Invoke(new IncomingBusDecorator(this, incomingPipeline, message), message, this.readOnlyConfiguration);
        }

        private class IncomingBusDecorator : IBusForHandler
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

            public Task Reply(object message, ReplyOptions options = null)
            {
                ReplyOptions replyOptions = GetOrCreateReplyOptions(this.incoming, options);
                return this.bus.Send(message, replyOptions, this.incoming);
            }

            public IDictionary<string, string> Headers(object message)
            {
                return this.incoming.Headers;
            }

            public void DoNotContinueDispatchingCurrentMessageToHandlers()
            {
                this.incomingPipeline.DoNotInvokeAnyMoreHandlers();
            }

            private static ReplyOptions GetOrCreateReplyOptions(TransportMessage incoming, ReplyOptions options = null)
            {
                Queue destination = incoming.ReplyTo;

                string correlationId = !string.IsNullOrEmpty(incoming.CorrelationId)
                    ? incoming.CorrelationId
                    : incoming.Id;

                if (options == null)
                {
                    return new ReplyOptions(destination, correlationId);
                }

                options.Queue = options.Queue ?? destination;
                options.CorrelationId = options.CorrelationId ?? correlationId;
                return options;
            }
        }
    }
}