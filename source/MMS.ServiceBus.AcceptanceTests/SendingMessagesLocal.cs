//-------------------------------------------------------------------------------
// <copyright file="SendingMessagesLocal.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Pipeline;
    using Testing;

    [TestFixture]
    public class SendingMessagesLocal
    {
        private Context context;

        private HandlerRegistrySimulator registry;

        private Broker broker;

        private MessageUnit sender;

        [SetUp]
        public void SetUp()
        {
            this.context = new Context();
            this.registry = new HandlerRegistrySimulator(this.context);

            this.broker = new Broker();
            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint("Sender").Concurrency(1))
                .Use(new AlwaysRouteToDestination(new Queue("Receiver")))
                .Use(this.registry);

            this.broker.Register(this.sender);

            this.broker.Start();
        }

        [TearDown]
        public void TearDown()
        {
            this.broker.Stop();
        }

        [Test]
        public async Task WhenOneMessageSentLocal_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.SendLocal(new Message { Bar = 42 });

            Assert.AreEqual(1, this.context.FooAsyncHandlerCalled);
            Assert.AreEqual(1, this.context.FooHandlerCalled);
        }

        [Test]
        public async Task WhenMultipeMessageSentLocal_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.SendLocal(new Message { Bar = 42 });
            await this.sender.SendLocal(new Message { Bar = 43 });

            Assert.AreEqual(2, this.context.FooAsyncHandlerCalled);
            Assert.AreEqual(2, this.context.FooHandlerCalled);
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