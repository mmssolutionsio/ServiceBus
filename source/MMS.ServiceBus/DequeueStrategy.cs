namespace MMS.ServiceBus
{
    using System;
    using System.Threading.Tasks;
    using System.Transactions;

    public class DequeueStrategy
    {
        private readonly EndpointConfiguration configuration;

        private readonly QueueClientListenerCreator clientListenerCreator;

        private AsyncClosable queueClient;

        private Func<TransportMessage, Task> onMessageAsync;

        public DequeueStrategy(EndpointConfiguration configuration, QueueClientListenerCreator clientListenerCreator)
        {
            this.clientListenerCreator = clientListenerCreator;
            this.configuration = configuration;
        }

        public async Task StartAsync(Func<TransportMessage, Task> onMessageAsync)
        {
            this.onMessageAsync = onMessageAsync;
            this.queueClient = await this.clientListenerCreator.StartAsync(this.configuration, this.OnMessageAsync);
        }

        public Task StopAsync()
        {
            return this.queueClient.CloseAsync();
        }

        private async Task OnMessageAsync(TransportMessage message)
        {
            try
            {
                if (this.configuration.IsTransactional)
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()/*, TransactionScopeAsyncFlowOption.Enabled */))
                    {
                        Transaction.Current.EnlistVolatile(new ReceiveResourceManager(message), EnlistmentOptions.None);

                        await this.onMessageAsync(message);

                        scope.Complete();
                    }
                }
                else
                {
                    await this.onMessageAsync(message);
                }
            }
            catch (Exception exception)
            {
                throw;
            }
        }
    }
}