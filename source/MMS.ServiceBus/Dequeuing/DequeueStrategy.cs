//-------------------------------------------------------------------------------
// <copyright file="DequeueStrategy.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Dequeuing
{
    using System;
    using System.Threading.Tasks;

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
            await this.onMessageAsync(message)
                               .ConfigureAwait(false);
        }
    }
}