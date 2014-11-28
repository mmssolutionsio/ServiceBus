using MMS.ServiceBus.Pipeline;

namespace MMS.ServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

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

        private QueueListenerSimulator simulator;

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
            this.simulator = new QueueListenerSimulator(this.incomingTransport);
            OutgoingPipelineFactory outgoingFactory = this.CreateOutgoingPipelineFactory();
            IncomingPipelineFactory incomingFactory = this.CreateIncomingPipelineFactory();

            this.unit = this.CreateBus(this.simulator, outgoingFactory, incomingFactory);
            await this.unit.StartAsync();
        }

        public async Task StopAsync()
        {
            await this.unit.StopAsync();
        }

        public IDisposable Scope()
        {
            return new Disposable(this);
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

        public void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            this.unit.DoNotContinueDispatchingCurrentMessageToHandlers();
        }

        protected virtual IncomingPipelineFactory CreateIncomingPipelineFactory()
        {
            return new UnitIncomingPipelineFactory(this.registry, this.IncomingLogical);
        }

        protected virtual OutgoingPipelineFactory CreateOutgoingPipelineFactory()
        {
            return new UnitOutgoingPipelineFactory(this.outgoing, this.OutgoingLogical);
        }

        protected virtual Bus CreateBus(QueueClientListenerCreator creator, OutgoingPipelineFactory outgoingPipelineFactory, IncomingPipelineFactory incomingPipelineFactory)
        {
            return new Bus(this.configuration, new DequeueStrategy(this.configuration, creator), new LogicalMessageFactory(), outgoingPipelineFactory, incomingPipelineFactory, this.router);
        }

        private class UnitOutgoingPipelineFactory : OutgoingPipelineFactory
        {
            private readonly ICollection<LogicalMessage> outgoing;

            private readonly Func<TransportMessage, Task> onMessage;

            public UnitOutgoingPipelineFactory(Func<TransportMessage, Task> onMessage, ICollection<LogicalMessage> outgoing)
            {
                this.onMessage = onMessage;
                this.outgoing = outgoing;
            }

            public override OutgoingPipeline Create()
            {
                var pipeline = new OutgoingPipeline();
                var senderStep = new DispatchToTransportPipelineStep(new MessageSenderSimulator(this.onMessage));
                return pipeline
                    .RegisterStep(new CreateTransportMessagePipelineStep())
                    .RegisterStep(new SerializeMessagePipelineStep(new DataContractMessageSerializer()))
                    .RegisterStep(new EnrichTransportMessageWithDestinationAddress())
                    .RegisterStep(senderStep)
                    .RegisterStep(new TraceOutgoingLogical(this.outgoing));
            }
        }

        private class UnitIncomingPipelineFactory : IncomingPipelineFactory
        {
            private readonly HandlerRegistry registry;

            private readonly ICollection<LogicalMessage> incoming;

            public UnitIncomingPipelineFactory(HandlerRegistry registry, ICollection<LogicalMessage> incoming)
                : base(registry)
            {
                this.incoming = incoming;
                this.registry = registry;
            }

            public override IncomingPipeline Create()
            {
                var pipeline = new IncomingPipeline();
                return pipeline
                    .RegisterStep(new DeserializeTransportMessagePipelineStep(new DataContractMessageSerializer(), new LogicalMessageFactory()))
                    .RegisterStep(new LoadMessageHandlers(this.registry))
                    .RegisterStep(new InvokeHandlers())
                    .RegisterStep(new TraceIncomingLogical(this.incoming));
            }
        }

        private class MessageSenderSimulator : MessageSender
        {
            private readonly Func<TransportMessage, Task> onMessage;

            public MessageSenderSimulator(Func<TransportMessage, Task> onMessage)
                : base(null)
            {
                this.onMessage = onMessage;
            }

            public override Task SendAsync(TransportMessage message, SendOptions options)
            {
                var brokeredMessage = message.ToBrokeredMessage();
                var transportMessage = new TransportMessage(brokeredMessage);

                return this.onMessage(transportMessage);
            }
        }

        private class QueueListenerSimulator : QueueClientListenerCreator
        {
            private readonly ICollection<TransportMessage> collector;

            private Func<TransportMessage, Task> onMessage;

            public QueueListenerSimulator(ICollection<TransportMessage> collector)
            {
                this.collector = collector;
            }

            public override Task<AsyncClosable> StartAsync(EndpointConfiguration configuration, Func<TransportMessage, Task> onMessage)
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

        private class TraceIncomingLogical : IIncomingLogicalPipelineStep
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

        private class TraceOutgoingLogical : IOutgoingLogicalPipelineStep
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

        private class Disposable : IDisposable
        {
            private readonly MessageUnit unit;

            public Disposable(MessageUnit unit)
            {
                this.unit = unit;
            }

            public void Dispose()
            {
                this.unit.OutgoingTransport.Clear();
                this.unit.IncomingTransport.Clear();
                this.unit.IncomingLogical.Clear();
                this.unit.OutgoingLogical.Clear();
            }
        }
    }
}