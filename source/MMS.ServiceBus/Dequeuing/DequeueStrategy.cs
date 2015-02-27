//-------------------------------------------------------------------------------
// <copyright file="DequeueStrategy.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Dequeuing
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;

    public class DequeueStrategy : IDequeueStrategy
    {
        private readonly IReceiveMessages receiveMessages;

        private EndpointConfiguration.ReadOnly configuration;

        private AsyncClosable receiver;

        private Func<TransportMessage, ITransaction, Task> onMessageAsync;

        private ITransactionalBusProvider transactionalBusProvider;

        public DequeueStrategy(IReceiveMessages receiveMessages)
        {
            this.receiveMessages = receiveMessages;
        }

        public async Task StartAsync(EndpointConfiguration.ReadOnly configuration, ITransactionalBusProvider transactionalBusProvider, Func<TransportMessage, ITransaction, Task> onMessage)
        {
            this.transactionalBusProvider = transactionalBusProvider;
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
            ExceptionDispatchInfo info = null;
            ITransaction tx = this.configuration.IsTransactional ? 
                this.transactionalBusProvider.BeginTransaction() : new ImmediateCompleteTransaction();

            try
            {
                await this.onMessageAsync(message, tx)
                    .ConfigureAwait(false);

                await tx.CompleteAsync();
            }
            catch (Exception exception)
            {
                info = ExceptionDispatchInfo.Capture(exception);
            }

            if (info != null)
            {
                await tx.RollbackAsync();

                info.Throw();
            }
        }
    }
}