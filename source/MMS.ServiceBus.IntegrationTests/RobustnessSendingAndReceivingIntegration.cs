//-------------------------------------------------------------------------------
// <copyright file="RobustnessSendingAndReceivingIntegration.cs" company="MMS AG">
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
    /// In this scenario I fire 1000 messages with 100 ms delay against the server and interrupt the network connection to the server for
    /// approximately 10 seconds after 45 seconds. This only works on Win8 or greater or WinServer2012 or greater.
    /// </summary>
    /// <code>
    ///     Start-Sleep -Seconds 45
    ///
    ///     Write-Host "Disable"
    ///
    ///     Disable-NetAdapter YourAdapter -Confirm:$false
    ///
    ///     Start-Sleep -Seconds 10
    ///
    ///     Write-Host "Enable"
    ///
    ///     Enable-NetAdapter YourAdapter -Confirm:$false
    /// </code>
    [TestFixture]
    [Category("Manual")]
    public class RobustnessSendingAndReceivingIntegration
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

        [Test]
        public async Task Send1000Messages_InvokesSynchronousAndAsynchronousHandlers()
        {
#pragma warning disable 4014
            Task.Run(async () =>
#pragma warning restore 4014
            {
                for (int i = 0; i < 1000; i++)
                {
                    await this.sender.Send(new Message { Bar = i });

                    await Task.Delay(100);
                }
            });
            
            await this.context.Wait(asyncHandlerCalls: 1000, handlersCalls: 1000);

            this.context.AsyncHandlerCalls.Should().BeInvoked(ntimes: 1000);
            this.context.HandlerCalls.Should().BeInvoked(ntimes: 1000);
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
                Debug.WriteLine("Async {0}", message.Bar);
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
                Debug.WriteLine("Sync {0}", message.Bar);
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

            public Task Wait(int asyncHandlerCalls, int handlersCalls)
            {
                var task1 = Task.Run(() => SpinWait.SpinUntil(() => this.AsyncHandlerCalls >= asyncHandlerCalls && this.HandlerCalls >= handlersCalls));
                var task2 = Task.Delay(TimeSpan.FromSeconds(200));

                return Task.WhenAny(task1, task2);
            }
        }
    }
}