//-------------------------------------------------------------------------------
// <copyright file="ReplyingToMessages.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
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
    using Pipeline;
    using Testing;

    [TestFixture]
    public class ReplyingToMessages
    {
        private const string SenderEndpointName = "Sender";
        private const string ReceiverEndpointName = "Receiver";
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
            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint(SenderEndpointName).Concurrency(1))
                .Use(new AlwaysRouteToDestination(Queue.Create(ReceiverEndpointName)))
                .Use(this.registry);

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverEndpointName).Concurrency(1))
                .Use(new AlwaysRouteToDestination(Queue.Create(SenderEndpointName)))
                .Use(this.registry);

            this.broker.Register(this.sender)
           .Register(this.receiver);

            this.broker.Start();
        }

        [Test]
        public async Task WhenOneMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.Send(new Message { Bar = 42 });

            this.context.AsyncHandlerCalls.Should().BeInvokedOnce();
            this.context.HandlerCalls.Should().BeInvokedOnce();
            this.context.ReplyHandlerCalls.Should().BeInvokedTwice();
        }

        [Test]
        public async Task WhenMultipeMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.Send(new Message { Bar = 42 });
            await this.sender.Send(new Message { Bar = 43 });
            await this.sender.Send(new Message { Bar = 44 });
            await this.sender.Send(new Message { Bar = 45 });

            this.context.AsyncHandlerCalls.Should().BeInvoked(ntimes: 4);
            this.context.HandlerCalls.Should().BeInvoked(ntimes: 4);
            this.context.ReplyHandlerCalls.Should().BeInvoked(ntimes: 8);
        }

        [TearDown]
        public void TearDown()
        {
            this.broker.Stop();
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
                this.context.ReplyHandlerCalls += 1;
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

            public async Task Handle(Message message, IBus bus)
            {
                this.context.AsyncHandlerCalls += 1;
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
                this.context.HandlerCalls += 1;
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
            public int AsyncHandlerCalls { get; set; }

            public int HandlerCalls { get; set; }

            public int ReplyHandlerCalls { get; set; }
        }
    }
}