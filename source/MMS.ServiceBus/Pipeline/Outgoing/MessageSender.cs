//-------------------------------------------------------------------------------
// <copyright file="MessageSender.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using ServiceBusMessageSender = Microsoft.ServiceBus.Messaging.MessageSender;

    public class MessageSender : ISendMessages
    {
        private readonly ConcurrentDictionary<Queue, ServiceBusMessageSender> senderCache = 
            new ConcurrentDictionary<Queue, ServiceBusMessageSender>(); 

        private readonly MessagingFactory factory;

        public MessageSender(MessagingFactory factory)
        {
            this.factory = factory;
        }

        public async Task SendAsync(TransportMessage message, SendOptions options)
        {
            ServiceBusMessageSender sender;
            if (!this.senderCache.TryGetValue(options.Queue, out sender))
            {
                sender = await this.factory.CreateMessageSenderAsync(options.Destination())
                    .ConfigureAwait(false);
                this.senderCache.TryAdd(options.Queue, sender);
            }

            await sender.SendAsync(message.ToBrokeredMessage())
                .ConfigureAwait(false);
        }

        public async Task CloseAsync()
        {
            foreach (var sender in this.senderCache.Values)
            {
                await sender.CloseAsync()
                    .ConfigureAwait(false);
            }

            this.senderCache.Clear();
        }
    }
}