//-------------------------------------------------------------------------------
// <copyright file="MessagePublisher.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public class MessagePublisher : IPublishMessages
    {
        private readonly MessagingFactory factory;

        public MessagePublisher(MessagingFactory factory)
        {
            this.factory = factory;
        }

        public async Task PublishAsync(TransportMessage message, PublishOptions options)
        {
            var publisher = await this.factory.CreateMessageSenderAsync(options.Destination);
            await publisher.SendAsync(message.ToBrokeredMessage());
        }
    }
}