//-------------------------------------------------------------------------------
// <copyright file="DequeueStrategy.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Threading.Tasks;
    using System.Transactions;

    public class DequeueStrategy
    {
        private readonly EndpointConfiguration configuration;

        private readonly IReceiveMessages receiveMessages;

        private AsyncClosable receiver;

        private Func<TransportMessage, Task> onMessageAsync;

        public DequeueStrategy(EndpointConfiguration configuration, IReceiveMessages receiveMessages)
        {
            this.receiveMessages = receiveMessages;
            this.configuration = configuration;
        }

        public async Task StartAsync(Func<TransportMessage, Task> onMessage)
        {
            this.onMessageAsync = onMessage;
            this.receiver = await this.receiveMessages.StartAsync(this.configuration, this.OnMessageAsync);
        }

        public Task StopAsync()
        {
            return this.receiver.CloseAsync();
        }

        private async Task OnMessageAsync(TransportMessage message)
        {
            if (this.configuration.IsTransactional)
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() /*, TransactionScopeAsyncFlowOption.Enabled */))
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
    }
}