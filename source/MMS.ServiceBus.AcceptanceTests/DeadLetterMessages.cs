//-------------------------------------------------------------------------------
// <copyright file="DeadLetterMessages.cs" company="Multimedia Solutions AG">
//   Copyright (c) MMS AG 2011-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NUnit.Framework;
    using ServiceBus.Pipeline;
    using ServiceBus.Testing;

    [TestFixture]
    public class DeadLetterMessages
    {
        private HandlerRegistrySimulator registry;

        private Broker broker;

        private MessageUnit sender;

        private MessageUnit receiver;

        [SetUp]
        public void SetUp()
        {
            this.registry = new HandlerRegistrySimulator();

            this.broker = new Broker();
            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint("Sender").Concurrency(1))
                .Use(new AlwaysRouteToDestination(Queue.Create("Receiver")));
            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint("Receiver")
                .Concurrency(1)).Use(this.registry);

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
        public void WhenMessageSentWithBodyWhichCannotBeDeserialized_MessageIsDeadlettered()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("{ ; }");
            writer.Flush();
            stream.Position = 0;

            var tm = new DeadLetterTransportMessage { MessageType = typeof(Message).AssemblyQualifiedName };
            tm.SetBody(stream);

            Func<Task> action = () => this.receiver.HandOver(tm);

            action.ShouldThrow<SerializationException>();
            tm.DeadLetterHeaders.Should().NotBeEmpty();
        }

        [Test]
        public void WhenMessageReachesMaximumNumberOfRetries_MessageIsDeadlettered()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("{ Bar: 1 }");
            writer.Flush();
            stream.Position = 0;

            var tm = new DeadLetterTransportMessage { MessageType = typeof(Message).AssemblyQualifiedName };
            tm.SetBody(stream);

            Func<Task> action = () => this.receiver.HandOver(tm);

            action.ShouldThrow<InvalidOperationException>();
            tm.DeadLetterHeaders.Should().NotBeEmpty();
        }

        public class DeadLetterTransportMessage : TransportMessage
        {
            public IDictionary<string, object> DeadLetterHeaders { get; private set; }

            protected override Task DeadLetterAsyncInternal(IDictionary<string, object> deadLetterHeaders)
            {
                this.DeadLetterHeaders = deadLetterHeaders;

                return Task.FromResult(0);
            }

            public override int DeliveryCount
            {
                get { return 10; }
            }
        }

        public class HandlerRegistrySimulator : HandlerRegistry
        {
            public override IReadOnlyCollection<object> GetHandlers(Type messageType)
            {
                if (messageType == typeof(Message))
                {
                    return this.HandleWith(new AsyncHandlerWhichFailsAllTheTime());
                }

                return this.DontHandle();
            }
        }

        public class AsyncHandlerWhichFailsAllTheTime : IHandleMessageAsync<Message>
        {
            public Task Handle(Message message, IBusForHandler bus)
            {
                throw new InvalidOperationException();
            }
        }

        public class Message
        {
            public int Bar { get; set; }
        }
    }
}