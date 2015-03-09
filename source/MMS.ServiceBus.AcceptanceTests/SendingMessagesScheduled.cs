// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendingMessagesScheduled.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NUnit.Framework;
    using Pipeline;
    using Testing;

    [TestFixture]
    public class SendingMessagesScheduled
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
                .Use(new AlwaysRouteToDestination(Queue.Create("Receiver")));
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
        public async Task WhenMessageSentScheduled_ThenSentBrokeredMessageContainsScheduledEnqueueTimeUtc()
        {
            var scheduledEnqueueTimeUtc = DateTime.MinValue + 2.Seconds();

            await this.sender.Send(new Message { Bar = 42 }, new SendOptions { ScheduledEnqueueTimeUtc = scheduledEnqueueTimeUtc });

            this.sender.OutgoingTransport.First().ToBrokeredMessage().ScheduledEnqueueTimeUtc.Should().Be(scheduledEnqueueTimeUtc);
            this.context.AsyncHandlerCalled.Should().BeInvokedOnce();
            this.context.HandlerCalled.Should().BeInvokedOnce();
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
                this.context.AsyncHandlerCalled += 1;
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
                this.context.HandlerCalled += 1;
            }
        }

        public class Message
        {
            public int Bar { get; set; }
        }

        public class Context
        {
            public int AsyncHandlerCalled { get; set; }

            public int HandlerCalled { get; set; }

            public IDictionary<string, string> AsyncHandlerCaughtHeaders { get; set; }

            public IDictionary<string, string> HandlerCaughtHeaders { get; set; }
        }
    }
}