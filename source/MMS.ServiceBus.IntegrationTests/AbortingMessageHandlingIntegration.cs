//-------------------------------------------------------------------------------
// <copyright file="AbortingMessageHandlingIntegration.cs" company="Multimedia Solutions AG">
//   Copyright (c) MMS AG 2011-2015
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
    using ServiceBus.Pipeline;
    using ServiceBus.Testing;

    [TestFixture]
    public class AbortingMessageHandlingIntegration
    {
        private const string SenderEndpointName = "Sender";
        private const string ReceiverEndpointName = "Receiver";

        private Context context;

        private HandlerRegistrySimulator registry;

        private MessageUnit sender;

        private MessageUnit receiver;

        [SetUp]
        public void SetUp()
        {
            this.context = new Context();
            this.registry = new HandlerRegistrySimulator(this.context);

            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint(SenderEndpointName).Concurrency(1))
                .Use(MessagingFactory.Create())
                .Use(new AlwaysRouteToDestination(Queue.Create(ReceiverEndpointName)))
                .Use(this.registry);

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverEndpointName).Concurrency(1))
                .Use(MessagingFactory.Create())
                .Use(this.registry);

            this.SetUpNecessaryInfrastructure();

            this.sender.StartAsync().Wait();
            this.receiver.StartAsync().Wait();
        }

        [TearDown]
        public void TearDown()
        {
            this.sender.StopAsync().Wait();
            this.receiver.StopAsync().Wait();
        }

        [Test]
        public async Task WhenPipelineAbortedAsync_ShouldNotContinueToSyncHandler()
        {
            await this.sender.Send(new Message { AbortAsync = true, AbortSync = false, Bar = 42 });

            await this.context.Wait(asyncHandlerCalls: 1, handlerCalls: 0, lastHandlerCalls: 0);

            this.context.AsyncHandlerCalls.Should().BeInvokedOnce();
            this.context.HandlerCalls.Should().NotBeInvoked();
            this.context.LastHandlerCalls.Should().NotBeInvoked();
        }

        [Test]
        public async Task WhenPipelineAbortedSync_ShouldNotContinueToLastHandler()
        {
            await this.sender.Send(new Message { AbortAsync = false, AbortSync = true, Bar = 42 });

            await this.context.Wait(asyncHandlerCalls: 1, handlerCalls: 1, lastHandlerCalls: 0);

            this.context.AsyncHandlerCalls.Should().BeInvokedOnce();
            this.context.HandlerCalls.Should().BeInvokedOnce();
            this.context.LastHandlerCalls.Should().NotBeInvoked();
        }

        [Test]
        public async Task WhenPipelineNotAborted_ShouldExecuteAllHandler()
        {
            await this.sender.Send(new Message { AbortAsync = false, AbortSync = false, Bar = 42 });

            await this.context.Wait(asyncHandlerCalls: 1, handlerCalls: 1, lastHandlerCalls: 1);

            this.context.AsyncHandlerCalls.Should().BeInvokedOnce();
            this.context.HandlerCalls.Should().BeInvokedOnce();
            this.context.LastHandlerCalls.Should().BeInvokedOnce();
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
            private readonly Context context;

            public HandlerRegistrySimulator(Context context)
            {
                this.context = context;
            }

            public override IReadOnlyCollection<object> GetHandlers(Type messageType)
            {
                if (messageType == typeof(Message))
                {
                    return this.HandleWith(
                        new AsyncMessageHandler(this.context),
                        new SyncAsAsyncHandlerDecorator<Message>(new MessageHandler(this.context)),
                        new LastHandler(this.context));
                }

                return this.DontHandle();
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

                if (message.AbortAsync)
                {
                    bus.DoNotContinueDispatchingCurrentMessageToHandlers();
                }

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

                if (message.AbortSync)
                {
                    bus.DoNotContinueDispatchingCurrentMessageToHandlers();
                }
            }
        }

        public class LastHandler : IHandleMessageAsync<Message>
        {
            private readonly Context context;

            public LastHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Message message, IBusForHandler bus)
            {
                this.context.LastHandlerCalled();
                return Task.FromResult(0);
            }
        }

        public class Message
        {
            public bool AbortAsync { get; set; }

            public bool AbortSync { get; set; }

            public int Bar { get; set; }
        }

        public class Context
        {
            private long asyncHandlerCalled;
            private long handlerCalled;
            private long lastHandlerCalled;

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

            public int LastHandlerCalls
            {
                get
                {
                    return (int)Interlocked.Read(ref this.lastHandlerCalled);
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

            public void LastHandlerCalled()
            {
                Interlocked.Increment(ref this.lastHandlerCalled);
            }

            public Task Wait(int asyncHandlerCalls, int handlerCalls, int lastHandlerCalls)
            {
                var task1 = Task.Run(() => SpinWait.SpinUntil(() => this.AsyncHandlerCalls >= asyncHandlerCalls && this.HandlerCalls >= handlerCalls && this.LastHandlerCalls >= lastHandlerCalls));
                var task2 = Task.Delay(TimeSpan.FromSeconds(60));

                return Task.WhenAny(task1, task2);
            }
        }
    }
}