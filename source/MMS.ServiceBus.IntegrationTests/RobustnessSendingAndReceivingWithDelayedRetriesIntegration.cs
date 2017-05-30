//-------------------------------------------------------------------------------
// <copyright file="RobustnessSendingAndReceivingWithDelayedRetriesIntegration.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Pipeline;
    using Testing;

    /// <summary>
    /// In this scenario I fire 1000 messages against the server which has a handler which throws for certain messages.
    /// </summary>
    [TestFixture]
    [Category("Manual")]
    public class RobustnessSendingAndReceivingWithDelayedRetriesIntegration
    {
        private const int MaxDelayedRetryCount = 2;
        private const int MaxImmediateRetryCount = 2;
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

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverEndpointName)
                .MaximumImmediateRetryCount(MaxImmediateRetryCount).MaximumDelayedRetryCount(MaxDelayedRetryCount))
                .Use(MessagingFactory.Create())
                .Use(this.registry);

            this.SetUpNecessaryInfrastructure();

            this.sender.StartAsync().Wait();
            this.receiver.StartAsync().Wait();
        }

        [Test]
        public async Task Send100AsyncMessages_WhenHandlerThrowsException_DeadLetters()
        {
#pragma warning disable 4014
            Task.Run(async () =>
#pragma warning restore 4014
            {
                for (int i = 0; i < 100; i++)
                {
                    await this.sender.Send(new AsyncMessage { Bar = i });
                }
            });

            await this.context.Wait(handlerCalls: 50);
            this.context.HandlerCalls.Should().BeInvoked(ntimes: 50);


            // Wait a bit in order to let the bus deadletter
            await Task.Delay(10000 * MaxDelayedRetryCount * MaxImmediateRetryCount);

            MessageReceiver deadLetterReceiver = await MessagingFactory.Create()
                .CreateMessageReceiverAsync(QueueClient.FormatDeadLetterPath(ReceiverEndpointName), ReceiveMode.ReceiveAndDelete);
            IEnumerable<BrokeredMessage> deadLetteredMessages = await deadLetterReceiver.ReceiveBatchAsync(50);

            deadLetteredMessages.Should().HaveCount(50);
            deadLetteredMessages.Should().OnlyContain(b => b.DeliveryCount == MaxImmediateRetryCount
                                                           && int.Parse(b.Properties[HeaderKeys.DelayedDeliveryCount].ToString()) == MaxDelayedRetryCount);
        }

        [Test]
        public async Task Send100SyncMessages_WhenHandlerThrowsException_DeadLetters()
        {
#pragma warning disable 4014
            Task.Run(async () =>
#pragma warning restore 4014
            {
                for (int i = 0; i < 100; i++)
                {
                    await this.sender.Send(new Message { Bar = i });
                }
            });

            await this.context.Wait(handlerCalls: 50);
            this.context.HandlerCalls.Should().BeInvoked(ntimes: 50);

            // Wait a bit in order to let the bus deadletter
            await Task.Delay(10000 * MaxDelayedRetryCount * MaxImmediateRetryCount);

            MessageReceiver deadLetterReceiver = await MessagingFactory.Create()
                .CreateMessageReceiverAsync(QueueClient.FormatDeadLetterPath(ReceiverEndpointName), ReceiveMode.ReceiveAndDelete);
            IEnumerable<BrokeredMessage> deadLetteredMessages = await deadLetterReceiver.ReceiveBatchAsync(50);

            deadLetteredMessages.Should().HaveCount(50);
            deadLetteredMessages.Should().OnlyContain(b => b.DeliveryCount == MaxImmediateRetryCount 
                                      && int.Parse(b.Properties[HeaderKeys.DelayedDeliveryCount].ToString()) == MaxDelayedRetryCount);
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
                if (messageType == typeof(AsyncMessage))
                {
                    return this.ConsumeWith(new AsyncMessageHandler(this.context));
                }

                if (messageType == typeof(Message))
                {
                    return this.ConsumeWith(new MessageHandler(this.context).AsAsync());
                }

                return this.ConsumeAll();
            }
        }

        public class AsyncMessageHandler : IHandleMessageAsync<AsyncMessage>
        {
            private readonly Context context;

            public AsyncMessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(AsyncMessage message, IBusForHandler bus)
            {
                Debug.WriteLine("Async {0}", message.Bar);
                if (message.Bar % 2 == 0)
                {
                    throw new InvalidOperationException();
                }

                this.context.HandlerCalled();
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
                Debug.WriteLine("Sync {0}, delayed {1}", message.Bar, bus.Headers(message)[HeaderKeys.DelayedDeliveryCount]);
                if (message.Bar % 2 == 0)
                {
                    throw new InvalidOperationException();
                }

                this.context.HandlerCalled();
            }
        }

        public class AsyncMessage
        {
            public int Bar { get; set; }
        }

        public class Message
        {
            public int Bar { get; set; }
        }

        public class Context
        {
            private long handlerCalled;

            public int HandlerCalls
            {
                get
                {
                    return (int)Interlocked.Read(ref this.handlerCalled);
                }
            }

            public void HandlerCalled()
            {
                Interlocked.Increment(ref this.handlerCalled);
            }

            public Task Wait(int handlerCalls)
            {
                var task1 = Task.Run(() => SpinWait.SpinUntil(() => this.HandlerCalls >= handlerCalls));
                var task2 = Task.Delay(TimeSpan.FromSeconds(180));

                return Task.WhenAny(task1, task2);
            }
        }
    }
}