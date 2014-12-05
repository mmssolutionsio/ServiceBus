//-------------------------------------------------------------------------------
// <copyright file="PublishingMessages.cs" company="Multimedia Solutions AG">
//   Copyright (c) MMS AG 2011-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NUnit.Framework;
    using ServiceBus.Pipeline;
    using ServiceBus.Testing;

    [TestFixture]
    public class PublishingMessages
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
            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint("Publisher").Concurrency(1))
                .Use(new AlwaysRouteToDestination(new Topic("Subscriber")));
            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint("Subscriber")
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
        [Ignore("Currently not yet implemented")]
        public async Task WhenOneMessagePublished_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.Publish(new Event { Bar = 42 });

            this.context.FooAsyncHandlerCalled.Should().BeInvokedOnce();
            this.context.FooHandlerCalled.Should().BeInvokedOnce();
        }

        [Test]
        [Ignore("Currently not yet implemented")]
        public async Task WhenMultipeMessagePublished_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.Publish(new Event { Bar = 42 });
            await this.sender.Publish(new Event { Bar = 43 });

            this.context.FooAsyncHandlerCalled.Should().BeInvokedTwice();
            this.context.FooHandlerCalled.Should().BeInvokedTwice();
        }

        public class PublishMessageRouter : MessageRouter
        {
            private readonly MessageRouter fallback;

            public PublishMessageRouter(MessageRouter fallback)
            {
                this.fallback = fallback;
            }

            public override IReadOnlyCollection<Address> GetDestinationFor(Type messageType)
            {
                if (messageType == typeof(Event))
                {
                    return new ReadOnlyCollection<Address>(new List<Address> { new Topic(typeof(Event).FullName) });
                }

                return this.fallback.GetDestinationFor(messageType);
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
                if (messageType == typeof(Event))
                {
                    return
                        new ReadOnlyCollection<object>(
                            new List<object>
                                {
                                    new AsyncMessageHandler(this.context),
                                    new SyncAsAsyncHandlerDecorator<Event>(new MessageHandler(this.context)),
                                });
                }

                return new ReadOnlyCollection<object>(new List<object>());
            }
        }

        public class AsyncMessageHandler : IHandleMessageAsync<Event>
        {
            private readonly Context context;

            public AsyncMessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Event message, IBus bus)
            {
                this.context.FooAsyncHandlerCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class MessageHandler : IHandleMessage<Event>
        {
            private readonly Context context;

            public MessageHandler(Context context)
            {
                this.context = context;
            }

            public void Handle(Event message, IBus bus)
            {
                this.context.FooHandlerCalled += 1;
            }
        }

        public class Event
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