//-------------------------------------------------------------------------------
// <copyright file="ReplyingToMessagesIntegration.cs" company="MMS AG">
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
    public class ReplyingToMessagesIntegration
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

            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint(SenderEndpointName).Concurrency(1).Transactional())
                .Use(MessagingFactory.Create())
                .Use(new AlwaysRouteToDestination(Queue.Create(ReceiverEndpointName)))
                .Use(this.registry);

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverEndpointName).Concurrency(1).Transactional())
                .Use(new AlwaysRouteToDestination(Queue.Create(SenderEndpointName)))
                .Use(MessagingFactory.Create())
                .Use(this.registry);

            this.SetUpNecessaryInfrastructure();

            this.sender.StartAsync().Wait();
            this.receiver.StartAsync().Wait();
        }


        [Test]
        public async Task WhenOneMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.Send(new Message { Bar = 42 });

            await this.context.Wait(asyncHandlerCalls: 1, handlersCalls: 1, replyHandlerCalls: 2);

            this.context.AsyncHandlerCalls.Should().BeInvokedOnce();
            this.context.HandlerCalls.Should().BeInvokedOnce();
            this.context.ReplyHandlerCalls.Should().BeInvokedTwice();
        }

        [Test]
        public async Task WhenMultipeMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            ITransaction firstTransaction = this.sender.BeginTransaction();

            await this.sender.Participate(firstTransaction).Send(new Message { Bar = 42 });
            await this.sender.Participate(firstTransaction).Send(new Message { Bar = 43 });
            await this.sender.Send(new Message { Bar = 44 });
            await this.sender.Send(new Message { Bar = 45 });

            await firstTransaction.RollbackAsync();

            await this.context.Wait(asyncHandlerCalls: 20, handlersCalls: 0, replyHandlerCalls: 20);

            this.context.AsyncHandlerCalls.Should().BeInvoked(ntimes: 20);
            this.context.HandlerCalls.Should().NotBeInvoked();
            this.context.ReplyHandlerCalls.Should().BeInvoked(ntimes: 20);
            this.context.HeaderValue.Should().Be("Value");
        }

        [TearDown]
        public void TearDown()
        {
            this.receiver.StopAsync().Wait();
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

                if (messageType == typeof(ReplyMessage))
                {
                    return this.ConsumeWith(new ReplyMessageHandler(this.context));
                }

                return this.ConsumeAll();
            }
        }

        public class ReplyMessageHandler : IHandleMessageAsync<ReplyMessage>
        {
            private readonly Context context;

            public ReplyMessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(ReplyMessage message, IBusForHandler bus)
            {
                this.context.ReplyHandlerCalled();
                if (bus.Headers(message).ContainsKey("Key"))
                {
                    this.context.HeaderValue = bus.Headers(message)["Key"];
                }

                return Task.FromResult(0);
            }
        }

        public class AsyncMessageHandler : IHandleMessageAsync<Message>
        {
            private readonly Context context;

            public AsyncMessageHandler(Context context)
            {
                this.context = context;
            }

            public async Task Handle(Message message, IBusForHandler bus)
            {
                this.context.AsyncHandlerCalled();
                await bus.Reply(new ReplyMessage { Answer = "AsyncMessageHandler" });

                var tx = bus.BeginTransaction();

                var options = new ReplyOptions();
                options.Headers.Add("Key", "Value");
                await bus.Participate(tx)
                    .Reply(new ReplyMessage { Answer = "AsyncMessageHandlerWithHeaders" }, options);

                await tx.CompleteAsync();

                throw new Exception();
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
                bus.Reply(new ReplyMessage { Answer = "MessageHandler" }).Wait();
            }
        }

        public class Message
        {
            public int Bar { get; set; }
        }

        public class ReplyMessage
        {
            public string Answer { get; set; }
        }

        public class Context
        {
            private long asyncHandlerCalled;
            private long handlerCalled;
            private long replyHandlerCalled;

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

            public int ReplyHandlerCalls
            {
                get
                {
                    return (int)Interlocked.Read(ref this.replyHandlerCalled);
                }
            }

            public string HeaderValue { get; set; }

            public void AsyncHandlerCalled()
            {
                Interlocked.Increment(ref this.asyncHandlerCalled);
            }

            public void HandlerCalled()
            {
                Interlocked.Increment(ref this.handlerCalled);
            }

            public void ReplyHandlerCalled()
            {
                Interlocked.Increment(ref this.replyHandlerCalled);
            }

            public Task Wait(int asyncHandlerCalls, int handlersCalls, int replyHandlerCalls)
            {
                var task1 = Task.Run(() => SpinWait.SpinUntil(() => this.AsyncHandlerCalls >= asyncHandlerCalls && this.HandlerCalls >= handlersCalls && this.ReplyHandlerCalls >= replyHandlerCalls));
                var task2 = Task.Delay(TimeSpan.FromSeconds(60));

                return Task.WhenAny(task1, task2);
            }
        }
    }
}