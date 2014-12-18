//-------------------------------------------------------------------------------
// <copyright file="SendingMessagesLocalIntegration.cs" company="MMS AG">
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
    using FluentAssertions;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Pipeline;
    using Testing;

    [TestFixture]
    public class SendingMessagesLocalIntegration
    {
        private const string SenderEndpointName = "Sender";
        private const string ReceiverEndpointName = "Receiver";

        private Context context;

        private HandlerRegistrySimulator registry;

        private MessageUnit sender;

        [SetUp]
        public void SetUp()
        {
            this.context = new Context();

            this.registry = new HandlerRegistrySimulator(this.context);

            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint(SenderEndpointName).Concurrency(1))
                .Use(MessagingFactory.Create())
                .Use(new AlwaysRouteToDestination(Queue.Create(ReceiverEndpointName)))
                .Use(this.registry);

            this.SetUpNecessaryInfrastructure();

            this.sender.StartAsync().Wait();
        }

        [Test]
        public async Task WhenOneMessageSentLocal_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.SendLocal(new Message { Bar = 42 });

            await this.context.Wait(asyncHandlerCalls: 1, handlerCalls: 1);

            this.context.AsyncHandlerCalls.Should().BeInvokedOnce();
            this.context.HandlerCalls.Should().BeInvokedOnce();
        }

        [Test]
        public async Task WhenMultipeMessageSentLocal_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.SendLocal(new Message { Bar = 42 });
            await this.sender.SendLocal(new Message { Bar = 43 });
            await this.sender.SendLocal(new Message { Bar = 44 });
            await this.sender.SendLocal(new Message { Bar = 45 });

            await this.context.Wait(asyncHandlerCalls: 4, handlerCalls: 4);

            this.context.AsyncHandlerCalls.Should().BeInvoked(ntimes: 4);
            this.context.HandlerCalls.Should().BeInvoked(ntimes: 4);
        }

        [TearDown]
        public void TearDown()
        {
            this.sender.StopAsync().Wait();
        }

        private void SetUpNecessaryInfrastructure()
        {
            var manager = NamespaceManager.Create();
            if (manager.QueueExists(SenderEndpointName))
            {
                manager.DeleteQueue(SenderEndpointName);
            }

            manager.CreateQueue(SenderEndpointName);

            if (manager.QueueExists(ReceiverEndpointName))
            {
                manager.DeleteQueue(ReceiverEndpointName);
            }

            manager.CreateQueue(ReceiverEndpointName);
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
                if (messageType == typeof(Message))
                {
                    return this.ConsumeWith(
                        new AsyncMessageHandler(this.context),
                        new MessageHandler(this.context).AsAsync());
                }

                return this.ConsumeAll();
            }
        }

        public class AsyncMessageHandler : IHandleMessageAsync<Message>
        {
            private readonly Context context;

            public AsyncMessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Message message, IBusForHandler bus)
            {
                this.context.AsyncHandlerCalled();
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

            public void Handle(Message message, IBusForHandler bus)
            {
                this.context.HandlerCalled();
            }
        }

        public class Message
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

            public Task Wait(int asyncHandlerCalls, int handlerCalls)
            {
                var task1 = Task.Run(() => SpinWait.SpinUntil(() => this.AsyncHandlerCalls >= asyncHandlerCalls && this.HandlerCalls >= handlerCalls));
                var task2 = Task.Delay(TimeSpan.FromSeconds(60));

                return Task.WhenAny(task1, task2);
            }
        }
    }

}