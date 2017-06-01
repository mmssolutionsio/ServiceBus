//-------------------------------------------------------------------------------
// <copyright file="DelayedRetryMessagesIntegration.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Pipeline;

    [TestFixture]
    public class DelayedRetryMessagesIntegration
    {
        private const string ReceiverEndpointName = "Receiver";

        private MessageUnit receiver;
        private MessagingFactory messagingFactory;

        private const int MaxImmediateRetryCount = 5;
        private const int MaxDelayedRetryCount = 3;

        [SetUp]
        public void SetUp()
        {
            this.messagingFactory = MessagingFactory.Create();

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverEndpointName).Concurrency(1).MaximumImmediateRetryCount(MaxImmediateRetryCount).MaximumDelayedRetryCount(MaxDelayedRetryCount))
                .Use(this.messagingFactory)
                .Use(new HandlerRegistrySimulator());

            this.SetUpNecessaryInfrastructure();

            this.receiver.StartAsync().Wait();
        }

        [Test]
        public async Task WhenMessageReachesMaximumNumberOfImmediateRetries_MessageIsDelayedUntilDeadlettered()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("{ Bar: 1 }");
            writer.Flush();
            stream.Position = 0;

            var tm = new TransportMessage { MessageType = typeof(Message).AssemblyQualifiedName };
            tm.SetBody(stream);

            MessageSender messageSender = await this.messagingFactory.CreateMessageSenderAsync(ReceiverEndpointName);
            await messageSender.SendAsync(tm.ToBrokeredMessage());

            MessageReceiver deadLetterReceiver = await this.messagingFactory.CreateMessageReceiverAsync(QueueClient.FormatDeadLetterPath(ReceiverEndpointName), ReceiveMode.ReceiveAndDelete);
            IEnumerable<BrokeredMessage> deadLetteredMessages = await deadLetterReceiver.ReceiveBatchAsync(MaxDelayedRetryCount);

            // That's not really a good assertion here. But how far should I compare exception, stacktrace etc.
            deadLetteredMessages.Should().HaveCount(1);
            var brokeredMessage = deadLetteredMessages.Single();
            brokeredMessage.DeliveryCount.Should().Be(MaxImmediateRetryCount);
            var properties = brokeredMessage.Properties;
            properties.Where(p => p.Key.StartsWith(HeaderKeys.FailurePrefix, StringComparison.InvariantCultureIgnoreCase))
                .Should().NotBeEmpty();
            properties[HeaderKeys.DelayedDeliveryCount]
                .Should().Be(MaxDelayedRetryCount.ToString());
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

        public class HandlerRegistrySimulator : HandlerRegistry
        {
            public override IReadOnlyCollection<object> GetHandlers(Type messageType)
            {
                if (messageType == typeof(Message))
                {
                    return this.ConsumeWith(new AsyncHandlerWhichFailsAllTheTime());
                }

                return this.ConsumeAll();
            }
        }

        public class AsyncHandlerWhichFailsAllTheTime : IHandleMessageAsync<Message>
        {
            public Task Handle(Message message, IBusForHandler bus)
            {
                throw new InvalidOperationException();
            }
        }
    }
}