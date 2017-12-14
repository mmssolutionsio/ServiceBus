//-------------------------------------------------------------------------------
// <copyright file="PublishingMessagesIntegration.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Pipeline;

    [TestFixture]
    public class RenewLockMessageIntegration
    {
        private const string ReceiverEndpointName = "Receiver";

        private MessagingFactory messagingFactory;
        private MessageUnit receiver;

        private Context context;
        [SetUp]
        public void SetUp()
        {
            this.context = new Context();
            this.messagingFactory = MessagingFactory.Create();

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverEndpointName).Concurrency(1))
                .Use(this.messagingFactory)
                .Use(new RenewLockMessageIntegration.HandlerRegistrySimulator(this.context));

            this.SetUpNecessaryInfrastructure();

            this.receiver.StartAsync().Wait();
        }

        [TearDown]
        public void TearDown()
        {
            this.receiver.StopAsync().Wait();
        }

        private void SetUpNecessaryInfrastructure()
        {
            var manager = NamespaceManager.Create();

            if (manager.QueueExists(ReceiverEndpointName))
            {
                manager.DeleteQueue(ReceiverEndpointName);
            }

            var queueDescription = new QueueDescription(ReceiverEndpointName)
            {
                LockDuration = TimeSpan.FromSeconds(10)
            };
            var queue = manager.CreateQueue(queueDescription);

        }

        [Test]
        public async Task WhenMessageHadlingToLong_RenewLock_HoldsLockOnMessage()
        {
            await this.receiver.SendLocal(new Message {Bar = 1});

            await this.context.Wait(2, 2, TimeSpan.FromSeconds(20));

            this.context.HandleStart.Should().BeInvokedOnce();
            this.context.HandleDone.Should().BeInvokedOnce();
        }

        public class Message
        {
            public int Bar { get; set; }
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
                    return this.ConsumeWith(new AsyncHandlerWhichLongRun(this.context));
                }

                return this.ConsumeAll();
            }
        }

        public class AsyncHandlerWhichLongRun : IHandleMessageAsync<Message>
        {
            private readonly Context context;

            public AsyncHandlerWhichLongRun(Context context)
            {
                this.context = context;
            }
            public async Task Handle(Message message, IBusForHandler bus)
            {
                var time = TimeSpan.FromSeconds(6);
                IRenewLock renewLock = bus as IRenewLock;

                context.Start();
                await Task.Delay(time);
                renewLock?.RenewLock();
                await Task.Delay(time);
                this.context.Done();
            }
        }

        public class Context
        {
            private long handleStart;
            private long handleDone;

            public int HandleStart
            {
                get
                {
                    return (int)Interlocked.Read(ref this.handleStart);
                }
            }

            public int HandleDone
            {
                get
                {
                    return (int)Interlocked.Read(ref this.handleDone);
                }
            }

            public void Start()
            {
                Interlocked.Increment(ref this.handleStart);
            }

            public void Done()
            {
                Interlocked.Increment(ref this.handleDone);
            }

            public Task Wait(int start, int done, TimeSpan timeOut)
            {
                var task1 = Task.Run(() => SpinWait.SpinUntil(() => this.HandleStart >= start && this.HandleDone >= done));
                var task2 = Task.Delay(timeOut);

                return Task.WhenAny(task1, task2);
            }
        }
    }
}