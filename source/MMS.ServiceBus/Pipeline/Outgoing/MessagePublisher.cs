//-------------------------------------------------------------------------------
// <copyright file="MessagePublisher.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public class MessagePublisher
    {
        private readonly MessagingFactory factory;

        public MessagePublisher(MessagingFactory factory)
        {
            this.factory = factory;
        }

        public Task PublishAsync(TransportMessage message, PublishOptions options)
        {
            return null;
        }
    }
}