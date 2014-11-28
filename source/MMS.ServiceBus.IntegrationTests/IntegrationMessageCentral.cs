namespace MMS.ServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using ServiceBus.Pipeline;
    using ServiceBus.Testing;

    internal class IntegrationMessageCentral
    {
        private const string ReceiverEndpointName = "Receiver";

        private const string SenderEndpointName = "Sender";

        public IntegrationMessageCentral(HandlerRegistry handlerRegistry)
        {
            var senderMessageFactory = MessagingFactory.Create();
            var receiverMessageFactory = MessagingFactory.Create();

            SetupNecessaryInfrastructureOnServiceBus();

            var senderConfiguration = new EndpointConfiguration()
                .Endpoint(SenderEndpointName)
                .Concurrency(1);

            var receiverConfiguration = new EndpointConfiguration()
                .Endpoint(ReceiverEndpointName)
                .Concurrency(1);

            var senderOutgoingPipelineFactory = new OutgoingPipelineFactory(senderMessageFactory);
            var senderIncomingPipelineFactory = new IncomingPipelineFactory(new HandlerRegistry());

            this.Sender = new Bus(senderConfiguration, new DequeueStrategy(senderConfiguration, new QueueClientListenerCreator(senderMessageFactory)), new LogicalMessageFactory(), senderOutgoingPipelineFactory, senderIncomingPipelineFactory, new AlwaysRouteToDestination(new Queue(ReceiverEndpointName)));

            var receiverOutgoingPipelineFactory = new OutgoingPipelineFactory(receiverMessageFactory);
            var receiverIncomingPipelineFactory = new IncomingPipelineFactory(handlerRegistry);
            this.Receiver = new Bus(receiverConfiguration, new DequeueStrategy(receiverConfiguration, new QueueClientListenerCreator(receiverMessageFactory)), new LogicalMessageFactory(), receiverOutgoingPipelineFactory, receiverIncomingPipelineFactory, new AlwaysRouteToDestination(new Queue(SenderEndpointName)));
        }

        private static void SetupNecessaryInfrastructureOnServiceBus()
        {
            var manager = NamespaceManager.Create();
            if (!manager.QueueExists(SenderEndpointName))
            {
                manager.CreateQueue(SenderEndpointName);
            }

            if (!manager.QueueExists(ReceiverEndpointName))
            {
                manager.CreateQueue(ReceiverEndpointName);
            }
        }

        public Bus Sender
        {
            get; private set;
        }

        public Bus Receiver
        {
            get;
            private set;
        }

        public async Task StartAsync()
        {
            await this.Receiver.StartAsync();
            await this.Sender.StartAsync();
        }

        public async Task StopAsync()
        {
            await this.Receiver.StopAsync();
            await this.Sender.StopAsync();
        }
    }
}