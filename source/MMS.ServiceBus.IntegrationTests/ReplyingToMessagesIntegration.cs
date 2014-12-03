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

            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint(SenderEndpointName).Concurrency(1))
                .Use(MessagingFactory.Create())
                .Use(new AlwaysRouteToDestination(new Queue(ReceiverEndpointName)))
                .Use(this.registry);

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverEndpointName).Concurrency(1))
                .Use(new AlwaysRouteToDestination(new Queue(SenderEndpointName)))
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

            await this.context.Wait(1, 1, 2);

            Assert.AreEqual(1, this.context.AsyncHandlerCalls);
            Assert.AreEqual(1, this.context.HandlerCalls);
            Assert.AreEqual(2, this.context.ReplyHandlerCalls);
        }

        [Test]
        public async Task WhenMultipeMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.Send(new Message { Bar = 42 });
            await this.sender.Send(new Message { Bar = 43 });
            await this.sender.Send(new Message { Bar = 44 });
            await this.sender.Send(new Message { Bar = 45 });

            await this.context.Wait(4, 4, 8);

            Assert.AreEqual(4, this.context.AsyncHandlerCalls);
            Assert.AreEqual(4, this.context.HandlerCalls);
            Assert.AreEqual(8, this.context.ReplyHandlerCalls);
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
                    return new ReadOnlyCollection<object>(new List<object>
                        {
                            new AsyncMessageHandler(this.context),
                            new SyncAsAsyncHandlerDecorator<Message>(new MessageHandler(this.context)),
                        });
                }

                if (messageType == typeof(ReplyMessage))
                {
                    return new ReadOnlyCollection<object>(new List<object>
                    {
                        new ReplyMessageHandler(this.context),
                    });
                }

                return new ReadOnlyCollection<object>(new List<object>());
            }
        }

        public class ReplyMessageHandler : IHandleMessageAsync<ReplyMessage>
        {
            private readonly Context context;

            public ReplyMessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(ReplyMessage message, IBus bus)
            {
                this.context.ReplyHandlerCalled();
                return Task.Delay(0);
            }
        }

        public class AsyncMessageHandler : IHandleMessageAsync<Message>
        {
            private readonly Context context;

            public AsyncMessageHandler(Context context)
            {
                this.context = context;
            }

            public async Task Handle(Message message, IBus bus)
            {
                this.context.AsyncHandlerCalled();
                await bus.Reply(new ReplyMessage { Answer = "AsyncMessageHandler" });
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
                return Task.Run(
                        () => SpinWait.SpinUntil(() => this.AsyncHandlerCalls >= asyncHandlerCalls && this.HandlerCalls >= handlersCalls && this.ReplyHandlerCalls >= replyHandlerCalls));
            }
        }
    }
}