//-------------------------------------------------------------------------------
// <copyright file="DeadLetterMessages.cs" company="Multimedia Solutions AG">
//   Copyright (c) MMS AG 2011-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NUnit.Framework;
    using ServiceBus.Pipeline;
    using ServiceBus.Testing;

    [TestFixture]
    public class DelayedRetryMessages
    {
        private const int MaxImmediateRetryCount = 6;
        private const int MaxDelayedRetryCount = 3;
        private HandlerRegistrySimulator registry;

        private Broker broker;

        private MessageUnit sender;

        private MessageUnit receiver;

        static int actualDelayedRetryCount;

        [SetUp]
        public void SetUp()
        {
            this.registry = new HandlerRegistrySimulator();

            this.broker = new Broker();
            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint("Sender").Concurrency(1))
                .Use(new AlwaysRouteToDestination(Queue.Create("Receiver")));
            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint("Receiver")
                .Concurrency(1).MaximumImmediateRetryCount(MaxImmediateRetryCount).MaximumDelayedRetryCount(MaxDelayedRetryCount)).Use(this.registry);

            this.broker.Register(this.sender)
                       .Register(this.receiver);

            this.broker.Start();
        }

        [TearDown]
        public void TearDown()
        {
            this.broker.Stop();
        }

        [Test]
        public void WhenMessageReachesMaximumNumberOfImmediateRetries_MessageIsDelayed()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("{ Bar: 1 }");
            writer.Flush();
            stream.Position = 0;

            actualDelayedRetryCount = 0;
            var tm = new TestTransportMessage(typeof(Message).AssemblyQualifiedName);
            tm.SetBody(stream);

            Func<Task> action = () => this.receiver.HandOver(tm);

            action.ShouldNotThrow<InvalidOperationException>();
            tm.DeadLetterHeaders.Should().BeNull();
            tm.DelayedDeliveryCount.Should().Be(1);
        }

        [Test]
        public void WhenMessageReachesMaximumNumberOfDelayedRetries_MessageIsDeadlettered()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("{ Bar: 1 }");
            writer.Flush();
            stream.Position = 0;

            actualDelayedRetryCount = MaxDelayedRetryCount;
            var tm = new TestTransportMessage(typeof(Message).AssemblyQualifiedName);
            tm.SetBody(stream);

            Func<Task> action = () => this.receiver.HandOver(tm);

            action.ShouldThrow<InvalidOperationException>();
            tm.DeadLetterHeaders.Should().NotBeEmpty();
            tm.DelayedDeliveryCount.Should().Be(MaxDelayedRetryCount);
        }

        public class TestTransportMessage : TransportMessage
        {
            public TestTransportMessage(string messageType)
            {
                this.MessageType = messageType;
                this.DelayedDeliveryCount = actualDelayedRetryCount;
            }

            public IDictionary<string, object> DeadLetterHeaders { get; private set; }

            protected override Task DeadLetterAsyncInternal(IDictionary<string, object> deadLetterHeaders)
            {
                this.DeadLetterHeaders = deadLetterHeaders;

                return Task.FromResult(0);
            }

            public override int DeliveryCount => DelayedRetryMessages.MaxImmediateRetryCount;

        }

        public class HandlerRegistrySimulator : HandlerRegistry
        {
            public override IReadOnlyCollection<object> GetHandlers(Type messageType)
            {
                if (messageType == typeof(Message))
                {
                    return this.ConsumeWith(new AsyncHandlerWhichFailsUntilDelayedRetryCountIsReached());
                }

                return this.ConsumeAll();
            }
        }

        public class AsyncHandlerWhichFailsUntilDelayedRetryCountIsReached : IHandleMessageAsync<Message>
        {
            public Task Handle(Message message, IBusForHandler bus)
            {
                int delayedDeliveryCount;
                string delayedDeliveryCountString;
                bus.Headers(message).TryGetValue(HeaderKeys.DelayedDeliveryCount, out delayedDeliveryCountString);
                int.TryParse(delayedDeliveryCountString, out delayedDeliveryCount);
                if (delayedDeliveryCount == actualDelayedRetryCount)
                    throw new InvalidOperationException();

                return Task.FromResult(0);
            }
        }

        public class Message
        {
            public int Bar { get; set; }
        }
    }
}