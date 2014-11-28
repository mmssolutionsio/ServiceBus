namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Pipeline;

    public class Bus : IBus
    {
        private readonly EndpointConfiguration configuration;

        private readonly DequeueStrategy strategy;

        private readonly LogicalMessageFactory factory;

        private readonly MessageRouter router;

        private readonly OutgoingPipelineFactory outgoingPipelineFactory;

        private readonly IncomingPipelineFactory incomingPipelineFactory;

        public Bus(EndpointConfiguration configuration, DequeueStrategy strategy, LogicalMessageFactory factory, 
            OutgoingPipelineFactory outgoingPipelineFactory, IncomingPipelineFactory incomingPipelineFactory, 
            MessageRouter router)
        {
            this.incomingPipelineFactory = incomingPipelineFactory;
            this.outgoingPipelineFactory = outgoingPipelineFactory;

            this.router = router;
            this.factory = factory;
            this.configuration = configuration;
            this.strategy = strategy;
        }

        public async Task StartAsync()
        {
            await this.strategy.StartAsync(this.OnMessageAsync);
        }

        public Task Send(object message, SendOptions options = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "You cannot send null");
            }

            var sendOptions = options ?? new SendOptions(this.GetDestinationForSend(message));
            LogicalMessage msg = this.factory.Create(message, sendOptions.Headers);

            return this.SendMessage(msg, sendOptions);
        }

        public void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            // This has only an effect when called in an incoming pipeline.
        }

        public async Task StopAsync()
        {
            await this.strategy.StopAsync();
        }

        private async Task SendMessage(LogicalMessage message, SendOptions options)
        {
            if (options.ReplyToAddress == null)
            {
                options.ReplyToAddress = this.configuration.EndpointQueue;
            }

            OutgoingPipeline outgoingPipeline = this.outgoingPipelineFactory.Create();
            await outgoingPipeline.Invoke(message, options);
        }

        private Address GetDestinationForSend(object message)
        {
            IReadOnlyCollection<Address> destinations = this.router.GetDestinationFor(message.GetType());

            if (destinations.Count > 1)
            {
                throw new InvalidOperationException("Sends can only target one address.");
            }

            return destinations.Single();
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

            public Task Send(object message, SendOptions options = null)
            {
                return this.bus.Send(message, options);
            }

            public void DoNotContinueDispatchingCurrentMessageToHandlers()
            {
                this.incomingPipeline.DoNotInvokeAnyMoreHandlers();
            }
        }
    }
}