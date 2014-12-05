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
        private Context context;

        private HandlerRegistrySimulator registry;

        private Broker broker;

        private MessageUnit sender;

        private MessageUnit receiver;

        [SetUp]
        public void SetUp()
        {
            this.context = new Context();
            this.registry = new HandlerRegistrySimulator(this.context);

            this.broker = new Broker();
            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint("Sender").Concurrency(1))
                .Use(new AlwaysRouteToDestination(new Queue("Receiver")));
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

        public class DeadLetterTransportMessage : TransportMessage
        {
            public IDictionary<string, object> DeadLetterHeaders { get; private set; }

            protected override Task DeadLetterAsyncInternal(IDictionary<string, object> deadLetterHeaders)
            {
                this.DeadLetterHeaders = deadLetterHeaders;

                return Task.FromResult(0);
            }
        }

        public class HandlerRegistrySimulator : HandlerRegistry
        {
            private readonly Context context;

            public HandlerRegistrySimulator(Context context)
            {
                this.context = context;
            }

            public override IReadOnlyCollection<object> GetHandlers(Type messageType)
            {
                if (messageType == typeof(Message))
                {
                    return
                        new ReadOnlyCollection<object>(
                            new List<object>
                                {
                                    new AsyncMessageHandler(this.context),
                                    new SyncAsAsyncHandlerDecorator<Message>(new MessageHandler(this.context)),
                                });
                }

                return new ReadOnlyCollection<object>(new List<object>());
            }
        }

        public class AsyncMessageHandler : IHandleMessageAsync<Message>
        {
            private readonly Context context;

            public AsyncMessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Message message, IBus bus)
            {
                this.context.FooAsyncHandlerCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class MessageHandler : IHandleMessage<Message>
        {
            private readonly Context context;

            public MessageHandler(Context context)
            {
                this.context = context;
            }

            public void Handle(Message message, IBus bus)
            {
                this.context.FooHandlerCalled += 1;
            }
        }

        public class Message
        {
            public int Bar { get; set; }
        }

        public class Context
        {
            public int FooAsyncHandlerCalled { get; set; }

            public int FooHandlerCalled { get; set; }
        }
    }
}