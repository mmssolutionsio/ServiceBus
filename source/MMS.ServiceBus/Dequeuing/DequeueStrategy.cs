//-------------------------------------------------------------------------------
// <copyright file="DequeueStrategy.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Dequeuing
{
    using System;
    using System.Threading.Tasks;
    using System.Transactions;

    public class DequeueStrategy : IDequeueStrategy
    {
        private readonly IReceiveMessages receiveMessages;

        private EndpointConfiguration.ReadOnly configuration;

        private AsyncClosable receiver;

        private Func<TransportMessage, Task> onMessageAsync;

        public DequeueStrategy(IReceiveMessages receiveMessages)
        {
            this.receiveMessages = receiveMessages;
        }

        public async Task StartAsync(EndpointConfiguration.ReadOnly configuration, Func<TransportMessage, Task> onMessage)
        {
            this.configuration = configuration;
            this.onMessageAsync = onMessage;
            this.receiver = await this.receiveMessages.StartAsync(this.configuration, this.OnMessageAsync)
                .ConfigureAwait(false);
        }

        public Task StopAsync()
        {
            return this.receiver.CloseAsync();
        }

        private async Task OnMessageAsync(TransportMessage message)
        {
            using (var tx = new TransactionScopeDecorator(this.configuration))
            {
                await this.onMessageAsync(message)
                    .ConfigureAwait(false);

                tx.Complete();
            }
        }

        private sealed class TransactionScopeDecorator : IDisposable
        {
            private readonly TransactionScope scope;

            public TransactionScopeDecorator(EndpointConfiguration.ReadOnly configuration)
            {
                if (configuration.IsTransactional)
                {
                    this.scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                }
            }

            public void Complete()
            {
                if (this.scope != null)
                {
                    this.scope.Complete();
                }
            }

            public void Dispose()
            {
                if (this.scope != null)
                {
                    this.scope.Dispose();
                }
            }
        }
    }
}