//-------------------------------------------------------------------------------
// <copyright file="PublishingMessagesIntegration.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Pipeline;
    using Testing;

    [TestFixture]
    public class PublishingMessagesIntegration
    {
        private Context context;

        private HandlerRegistrySimulator registry;

        private MessageUnit sender;
        private MessageUnit receiver;

        [SetUp]
        public void SetUp()
        {
            this.context = new Context();

            this.registry = new HandlerRegistrySimulator(this.context);

            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint("Publisher").Concurrency(1))
                .Use(MessagingFactory.Create())
                .Use(new AlwaysRouteToDestination(new Topic("MMS.ServiceBus.PublishingMessagesIntegration.Event")))
                .Use(this.registry);

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint("Subscriber").Concurrency(1))
                .Use(MessagingFactory.Create())
                .Use(this.registry);

            this.sender.StartAsync().Wait();
            this.receiver.StartAsync().Wait();
        }

        [Test]
        public async Task WhenOneMessagePublished_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.Publish(new Event { Bar = 42 });

            await Task.Delay(100);

            await this.context.Wait(1);

            Assert.AreEqual(1, this.context.AsyncHandlerCalls);
            Assert.AreEqual(1, this.context.HandlerCalls);
        }

        [Test]
        public async Task WhenMultipeMessagesPublished_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.Publish(new Event { Bar = 42 });
            await this.sender.Publish(new Event { Bar = 43 });
            await this.sender.Publish(new Event { Bar = 44 });
            await this.sender.Publish(new Event { Bar = 45 });

            await Task.Delay(100);

            await this.context.Wait(4);

            Assert.AreEqual(4, this.context.AsyncHandlerCalls);
            Assert.AreEqual(4, this.context.HandlerCalls);
        }

        [TearDown]
        public void TearDown()
        {
            this.receiver.StopAsync().Wait();
            this.sender.StopAsync().Wait();
        }

        public class HandlerRegistrySimulator : HandlerRegistry
        {
            private Context context;

            public HandlerRegistrySimulator(Context context)
            {
                this.context = context;
            }

            public override IReadOnlyCollection<object> GetHandlers(Type messageType)
            {
                if (messageType == typeof(Event))
                {
                    return new ReadOnlyCollection<object>(new List<object>
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

            public async Task Handle(Event message, IBus bus)
            {
                this.context.AsyncHandlerCalled();
                await Task.Delay(20);
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
                this.context.HandlerCalled();
            }
        }

        public class Event
        {
            public int Bar { get; set; }
        }

        public class Context
        {
            private long asyncHandlerCalled;
            private long handlerCalled;

            public int AsyncHandlerCalls
            {
                get
                {
                    return (int)Interlocked.Read(ref this.asyncHandlerCalled);
                }
            }

            public int HandlerCalls
            {
                get
                {
                    return (int)Interlocked.Read(ref this.handlerCalled);
                }
            }

            public void AsyncHandlerCalled()
            {
                Interlocked.Increment(ref this.asyncHandlerCalled);
            }

            public void HandlerCalled()
            {
                Interlocked.Increment(ref this.handlerCalled);
            }

            public async Task Wait(int numberOfCalls)
            {
                await Task.Run(
                        () => SpinWait.SpinUntil(() => this.AsyncHandlerCalls >= numberOfCalls && this.HandlerCalls >= numberOfCalls))
                        .ConfigureAwait(false);
            }
        }
    }
}