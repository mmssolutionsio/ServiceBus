using MMS.ServiceBus.Pipeline;

namespace MMS.ServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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

        protected HandlerRegistry registry;

        protected MessageRouter router;

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

        public async Task StartAsync()
        {
            this.simulator = new MessageReceiverSimulator(this.incomingTransport);
            IOutgoingPipelineFactory outgoingFactory = this.CreateOutgoingPipelineFactory();
            IIncomingPipelineFactory incomingFactory = this.CreateIncomingPipelineFactory();

            this.unit = this.CreateBus(this.simulator, outgoingFactory, incomingFactory);
            await this.unit.StartAsync();
        }

        public async Task StopAsync()
        {
            await this.unit.StopAsync();
        }

        public void SetOutgoing(Func<TransportMessage, Task> outgoing)
        {
            this.outgoing = async msg =>
                {
                    this.outgoingTransport.Add(msg);
                    await outgoing(msg);
                };
        }

        public async Task HandOver(TransportMessage message)
        {
            await this.simulator.HandOver(message);
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
            return new UnitIncomingPipelineFactory(this.registry, this.IncomingLogical);
        }

        protected virtual IOutgoingPipelineFactory CreateOutgoingPipelineFactory()
        {
            return new UnitOutgoingPipelineFactory(this.outgoing, this.OutgoingLogical, this.router);
        }

        protected virtual Bus CreateBus(IReceiveMessages receiver, IOutgoingPipelineFactory outgoingPipelineFactory, IIncomingPipelineFactory incomingPipelineFactory)
        {
            return new Bus(this.configuration, new DequeueStrategy(this.configuration, receiver), new LogicalMessageFactory(), outgoingPipelineFactory, incomingPipelineFactory);
        }

        private class UnitOutgoingPipelineFactory : IOutgoingPipelineFactory
        {
            private readonly ICollection<LogicalMessage> outgoing;

            private readonly Func<TransportMessage, Task> onMessage;
            private readonly MessageRouter router;

            public UnitOutgoingPipelineFactory(Func<TransportMessage, Task> onMessage, ICollection<LogicalMessage> outgoing, MessageRouter router)
            {
                this.router = router;
                this.onMessage = onMessage;
                this.outgoing = outgoing;
            }

            public OutgoingPipeline Create()
            {
                var pipeline = new OutgoingPipeline();
                var senderStep = new DispatchToTransportStep(new MessageSenderSimulator(this.onMessage), new MessagePublisher(null));
                return pipeline
                    .RegisterStep(new CreateTransportMessageStep())
                    .RegisterStep(new SerializeMessageStep(new DataContractMessageSerializer()))
                    .RegisterStep(new DetermineDestinationStep(this.router))
                    .RegisterStep(new EnrichTransportMessageWithDestinationAddress())
                    .RegisterStep(senderStep)
                    .RegisterStep(new TraceOutgoingLogical(this.outgoing));
            }
        }

        private class UnitIncomingPipelineFactory : IIncomingPipelineFactory
        {
            private readonly HandlerRegistry registry;

            private readonly ICollection<LogicalMessage> incoming;

            public UnitIncomingPipelineFactory(HandlerRegistry registry, ICollection<LogicalMessage> incoming)
            {
                this.incoming = incoming;
                this.registry = registry;
            }

            public IncomingPipeline Create()
            {
                var pipeline = new IncomingPipeline();
                return pipeline
                    .RegisterStep(new DeserializeTransportMessageStep(new DataContractMessageSerializer(), new LogicalMessageFactory()))
                    .RegisterStep(new LoadMessageHandlersStep(this.registry))
                    .RegisterStep(new InvokeHandlersStep())
                    .RegisterStep(new TraceIncomingLogical(this.incoming));
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

        private class MessageReceiverSimulator : IReceiveMessages
        {
            private readonly ICollection<TransportMessage> collector;

            private Func<TransportMessage, Task> onMessage;

            public MessageReceiverSimulator(ICollection<TransportMessage> collector)
            {
                this.collector = collector;
            }

            public Task<AsyncClosable> StartAsync(EndpointConfiguration configuration, Func<TransportMessage, Task> onMessage)
            {
                this.onMessage = onMessage;

                return Task.FromResult(new AsyncClosable(() => Task.FromResult(0)));
            }

            public async Task HandOver(TransportMessage message)
            {
                this.collector.Add(message);
                await this.onMessage(message);
            }
        }

        private class TraceIncomingLogical : IIncomingLogicalStep
        {
            private readonly ICollection<LogicalMessage> collector;

            public TraceIncomingLogical(ICollection<LogicalMessage> collector)
            {
                this.collector = collector;
            }

            public async Task Invoke(IncomingLogicalContext context, IBus bus, Func<Task> next)
            {
                this.collector.Add(context.LogicalMessage);
                await next();
            }
        }

        private class TraceOutgoingLogical : IOutgoingLogicalStep
        {
            private readonly ICollection<LogicalMessage> collector;

            public TraceOutgoingLogical(ICollection<LogicalMessage> collector)
            {
                this.collector = collector;
            }

            public async Task Invoke(OutgoingLogicalContext context, Func<Task> next)
            {
                this.collector.Add(context.LogicalMessage);
                await next();
            }
        }
    }
}