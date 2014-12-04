//-------------------------------------------------------------------------------
// <copyright file="DeadLetterMessagesIntegration.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Pipeline;

    [TestFixture]
    public class DeadLetterMessagesIntegration
    {
        private const string ReceiverEndpointName = "Receiver";

        private MessageUnit receiver;
        private MessagingFactory messagingFactory;

        [SetUp]
        public void SetUp()
        {
            this.messagingFactory = MessagingFactory.Create();

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverEndpointName).Concurrency(1))
                .Use(this.messagingFactory)
                .Use(new HandlerRegistry());

            this.SetUpNecessaryInfrastructure();

            this.receiver.StartAsync().Wait();
        }

        [Test]
        public async Task WhenMessageSentWithBodyWhichCannotBeDeserialized_MessagesIsDeadlettered()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("{ ; }");
            writer.Flush();
            stream.Position = 0;

            var tm = new TransportMessage { MessageType = typeof(Message).AssemblyQualifiedName };
            tm.SetBody(stream);

            MessageSender sender = await this.messagingFactory.CreateMessageSenderAsync(ReceiverEndpointName);
            await sender.SendAsync(tm.ToBrokeredMessage());

            MessageReceiver deadLetterReceiver = await this.messagingFactory.CreateMessageReceiverAsync(QueueClient.FormatDeadLetterPath(ReceiverEndpointName), ReceiveMode.ReceiveAndDelete);
            IEnumerable<BrokeredMessage> deadLetteredMessages = await deadLetterReceiver.ReceiveBatchAsync(10);
            
            deadLetteredMessages.Should()
                .HaveCount(1)
                .And.ContainSingle(x => x.Properties.ContainsKey(HeaderKeys.DeadLetterDescription) && x.Properties.ContainsKey(HeaderKeys.DeadLetterReason));
        }

        [TearDown]
        public void TearDown()
        {
            this.receiver.StopAsync().Wait();
        }

        private void SetUpNecessaryInfrastructure()
        {
            var manager = NamespaceManager.Create();

            if (manager.QueueExists(ReceiverEndpointName))
            {
                manager.DeleteQueue(ReceiverEndpointName);
            }

            manager.CreateQueue(ReceiverEndpointName);
        }

        public class Message
        {
            public int Bar { get; set; }
        }
    }
}