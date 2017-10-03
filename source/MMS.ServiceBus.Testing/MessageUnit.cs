//-------------------------------------------------------------------------------
// <copyright file="MessageUnit.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dequeuing;
    using Pipeline;
    using Pipeline.Incoming;
    using Pipeline.Outgoing;

    public class MessageUnit : IBus
    {
        private readonly List<TransportMessage> outgoingTransport;

        private readonly List<TransportMessage> incomingTransport;

        private readonly List<LogicalMessage> incomingLogical;

        private readonly List<LogicalMessage> outgoingLogical;

        private readonly EndpointConfiguration configuration;

        private Bus unit;

        private MessageReceiverSimulator simulator;

        private Func<TransportMessage, Task> outgoing;

        public MessageUnit(EndpointConfiguration configuration)
        {
            this.configuration = configuration;

            this.outgoingTransport = new List<TransportMessage>();
            this.incomingTransport = new List<TransportMessage>();
            this.incomingLogical = new List<LogicalMessage>();
            this.outgoingLogical = new List<LogicalMessage>();
        }

        public Address Endpoint
        {
            get
            {
                return this.configuration.EndpointQueue;
            }
        }

        public List<TransportMessage> OutgoingTransport
        {
            get
            {
                return this.outgoingTransport;
            }
        }

        public List<TransportMessage> IncomingTransport
        {
            get
            {
                return this.incomingTransport;
            }
        }

        public List<LogicalMessage> IncomingLogical
        {
            get
            {
                return this.incomingLogical;
            }
        }

        public List<LogicalMessage> OutgoingLogical
        {
            get
            {
                return this.outgoingLogical;
            }
        }

        protected IHandlerRegistry Registry { get; set; }

        protected IMessageRouter Router { get; set; }

        public MessageUnit Use(HandlerRegistry registry)
        {
            this.Registry = registry;
            return this;
        }

        public MessageUnit Use(IMessageRouter router)
        {
            this.Router = router;
            return this;
        }

        public Task StartAsync()
        {
            this.simulator = new MessageReceiverSimulator(this.incomingTransport);
            IOutgoingPipelineFactory outgoingFactory = this.CreateOutgoingPipelineFactory();
            IIncomingPipelineFactory incomingFactory = this.CreateIncomingPipelineFactory();

            this.unit = this.CreateBus(this.simulator, outgoingFactory, incomingFactory);
            return this.unit.StartAsync();
        }

        public Task StopAsync()
        {
            return this.unit.StopAsync();
        }

        public void SetOutgoing(Func<TransportMessage, Task> outgoing)
        {
            this.outgoing = msg =>
                {
                    this.outgoingTransport.Add(msg);
                    return outgoing(msg);
                };
        }

        public Task SendLocal(object message)
        {
            return this.unit.SendLocal(message);
        }

        public Task HandOver(TransportMessage message)
        {
            return this.simulator.HandOver(message);
        }

        public Task Send(object message, SendOptions options = null)
        {
            return this.unit.Send(message, options);
        }

        public Task Publish(object message, PublishOptions options = null)
        {
            return this.unit.Publish(message, options);
        }

        protected virtual IIncomingPipelineFactory CreateIncomingPipelineFactory()
        {
            return new UnitIncomingPipelineFactory(this.Registry, this.IncomingLogical);
        }

        protected virtual IOutgoingPipelineFactory CreateOutgoingPipelineFactory()
        {
            return new UnitOutgoingPipelineFactory(this.outgoing, this.OutgoingLogical, this.Router);
        }

        protected virtual Bus CreateBus(IReceiveMessages receiver, IOutgoingPipelineFactory outgoingPipelineFactory, IIncomingPipelineFactory incomingPipelineFactory)
        {
            return new Bus(this.configuration, new DequeueStrategy(receiver), outgoingPipelineFactory, incomingPipelineFactory);
        }

        private class UnitOutgoingPipelineFactory : IOutgoingPipelineFactory
        {
            private readonly ICollection<LogicalMessage> outgoing;

            private readonly Func<TransportMessage, Task> onMessage;
            private readonly IMessageRouter router;

            public UnitOutgoingPipelineFactory(Func<TransportMessage, Task> onMessage, ICollection<LogicalMessage> outgoing, IMessageRouter router)
            {
                this.router = router;
                this.onMessage = onMessage;
                this.outgoing = outgoing;
            }

            public Task WarmupAsync()
            {
                return Task.FromResult(0);
            }

            public OutgoingPipeline Create()
            {
                var pipeline = new OutgoingPipeline();
                var senderStep = new DispatchToTransportStep(new MessageSenderSimulator(this.onMessage), new MessagePublisherSimulator(this.onMessage));

                pipeline.Logical
                    .Register(new CreateTransportMessageStep())
                    .Register(new TraceOutgoingLogical(this.outgoing));

                pipeline.Transport
                    .Register(new SerializeMessageStep(new NewtonsoftJsonMessageSerializer()))
                    .Register(new DetermineDestinationStep(this.router))
                    .Register(new EnrichTransportMessageWithDestinationAddress())
                    .Register(senderStep);

                return pipeline;
            }

            public Task CooldownAsync()
            {
                return Task.FromResult(0);
            }
        }

        private class UnitIncomingPipelineFactory : IIncomingPipelineFactory
        {
            private readonly IHandlerRegistry registry;

            private readonly ICollection<LogicalMessage> incoming;

            public UnitIncomingPipelineFactory(IHandlerRegistry registry, ICollection<LogicalMessage> incoming)
            {
                this.incoming = incoming;
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
                    .Register(new DeserializeTransportMessageStep(new NewtonsoftJsonMessageSerializer()));

                pipeline.Logical
                    .Register(new DeadLetterMessagesWhenDelayedRetryCountIsReachedStep())
                    .Register(new DelayMessagesWhenImmediateRetryCountIsReachedStep())
                    .Register(new DeadletterMessageImmediatelyExceptionStep())
                    .Register(new LoadMessageHandlersStep(this.registry))
                    .Register(new InvokeHandlerStep())
                    .Register(new TraceIncomingLogical(this.incoming));

                return pipeline;
            }

            public Task CooldownAsync()
            {
                return Task.FromResult(0);
            }
        }

        private class MessageSenderSimulator : ISendMessages
        {
            private readonly Func<TransportMessage, Task> onMessage;

            public MessageSenderSimulator(Func<TransportMessage, Task> onMessage)
            {
                this.onMessage = onMessage;
            }

            public Task SendAsync(TransportMessage message, SendOptions options)
            {
                var brokeredMessage = message.ToBrokeredMessage();
                var transportMessage = new TransportMessage(brokeredMessage);

                return this.onMessage(transportMessage);
            }
        }

        private class MessagePublisherSimulator : IPublishMessages
        {
            private readonly Func<TransportMessage, Task> onMessage;

            public MessagePublisherSimulator(Func<TransportMessage, Task> onMessage)
            {
                this.onMessage = onMessage;
            }

            public Task PublishAsync(TransportMessage message, PublishOptions options)
            {
                var brokeredMessage = message.ToBrokeredMessage();
                var transportMessage = new TransportMessage(brokeredMessage);

                return this.onMessage(transportMessage);
            }
        }

        private class MessageReceiverSimulator : IReceiveMessages
        {
            private readonly ICollection<TransportMessage> collector;

            private Func<TransportMessage, Task> onMessage;

            public MessageReceiverSimulator(ICollection<TransportMessage> collector)
            {
                this.collector = collector;
            }

            public Task<AsyncClosable> StartAsync(EndpointConfiguration.ReadOnly configuration, Func<TransportMessage, Task> onMessage)
            {
                this.onMessage = onMessage;

                return Task.FromResult(new AsyncClosable(() => Task.FromResult(0)));
            }

            public Task HandOver(TransportMessage message)
            {
                this.collector.Add(message);
                return this.onMessage(message);
            }
        }

        private class TraceIncomingLogical : IIncomingLogicalStep
        {
            private readonly ICollection<LogicalMessage> collector;

            public TraceIncomingLogical(ICollection<LogicalMessage> collector)
            {
                this.collector = collector;
            }

            public Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next)
            {
                this.collector.Add(context.LogicalMessage);
                return next();
            }
        }

        private class TraceOutgoingLogical : IOutgoingLogicalStep
        {
            private readonly ICollection<LogicalMessage> collector;

            public TraceOutgoingLogical(ICollection<LogicalMessage> collector)
            {
                this.collector = collector;
            }

            public Task Invoke(OutgoingLogicalContext context, Func<Task> next)
            {
                this.collector.Add(context.LogicalMessage);
                return next();
            }
        }
    }
}