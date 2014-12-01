//-------------------------------------------------------------------------------
// <copyright file="MessageSender.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public class MessageSender
    {
        private readonly MessagingFactory factory;

        public MessageSender(MessagingFactory factory)
        {
            this.factory = factory;
        }

        public virtual async Task SendAsync(TransportMessage message, SendOptions options)
        {
            var sender = await this.factory.CreateMessageSenderAsync(options.Destination);

            await sender.SendAsync(message.ToBrokeredMessage());
        }
    }
}