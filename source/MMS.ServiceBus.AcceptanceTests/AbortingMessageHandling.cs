//-------------------------------------------------------------------------------
// <copyright file="AbortingMessageHandling.cs" company="Multimedia Solutions AG">
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
    public class AbortingMessageHandling
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
        public async Task WhenPipelineAbortedAsync_ShouldNotContinueToSyncHandler()
        {
            await this.sender.Send(new Message { AbortAsync = true, AbortSync = false, Bar = 42 });

            this.context.AsyncHandlerCalled.Should().BeInvokedOnce();
            this.context.HandlerCalled.Should().NotBeInvoked();
            this.context.LastHandlerCalled.Should().NotBeInvoked();
        }

        [Test]
        public async Task WhenPipelineAbortedSync_ShouldNotContinueToLastHandler()
        {
            await this.sender.Send(new Message { AbortAsync = false, AbortSync = true, Bar = 42 });

            this.context.AsyncHandlerCalled.Should().BeInvokedOnce();
            this.context.HandlerCalled.Should().BeInvokedOnce();
            this.context.LastHandlerCalled.Should().NotBeInvoked();
        }

        [Test]
        public async Task WhenPipelineNotAborted_ShouldExecuteAllHandler()
        {
            await this.sender.Send(new Message { AbortAsync = false, AbortSync = false, Bar = 42 });

            this.context.AsyncHandlerCalled.Should().BeInvokedOnce();
            this.context.HandlerCalled.Should().BeInvokedOnce();
            this.context.LastHandlerCalled.Should().BeInvokedOnce();
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
                    return new ReadOnlyCollection<object>(new List<object>
                        {
                            new AsyncMessageHandler(this.context),
                            new SyncAsAsyncHandlerDecorator<Message>(new MessageHandler(this.context)),
                            new LastHandler(this.context),
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
                this.context.AsyncHandlerCalled += 1;

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

            public void Handle(Message message, IBus bus)
            {
                this.context.HandlerCalled += 1;

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

            public Task Handle(Message message, IBus bus)
            {
                this.context.LastHandlerCalled += 1;
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
            public int AsyncHandlerCalled { get; set; }

            public int HandlerCalled { get; set; }

            public int LastHandlerCalled { get; set; }
        }
    }
}