namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Pipeline;
    using Testing;

    [TestFixture]
    public class ReplyingToMessagesIntegration
    {
        private Context context;

        private HandlerRegistrySimulator registry;

        private MessageUnit sender;
        private MessageUnit receiver;

        [SetUp]
        public void SetUp()
        {
            this.context = new Context();

            this.registry = new HandlerRegistrySimulator(this.context);

            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint("Sender").Concurrency(1))
                .Use(MessagingFactory.Create())
                .Use(new AlwaysRouteToDestination(new Queue("Receiver")))
                .Use(this.registry);

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint("Receiver").Concurrency(1))
                .Use(MessagingFactory.Create())
                .Use(this.registry);

            this.sender.StartAsync().Wait();
            this.receiver.StartAsync().Wait();
        }

        [Test]
        public async Task WhenOneMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.Send(new Message { Bar = 42 });

            await Task.Delay(100);

            await this.context.Wait(1);

            Assert.AreEqual(1, this.context.AsyncHandlerCalls);
            Assert.AreEqual(1, this.context.HandlerCalls);
        }

        [Test]
        public async Task WhenMultipeMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.sender.Send(new Message { Bar = 42 });
            await this.sender.Send(new Message { Bar = 43 });
            await this.sender.Send(new Message { Bar = 44 });
            await this.sender.Send(new Message { Bar = 45 });

            await Task.Delay(100);

            await this.context.Wait(4);

            Assert.AreEqual(4, this.context.AsyncHandlerCalls);
            Assert.AreEqual(4, this.context.HandlerCalls);
        }

        [TearDown]
        public void TearDown()
        {
            this.receiver.StopAsync().Wait();
            this.sender.StopAsync().Wait();
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

            public async Task Handle(Message message, IBus bus)
            {
                this.context.AsyncHandlerCalled();
                await Task.Delay(0);
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

            public async Task Wait(int numberOfCalls)
            {
                await Task.Run(
                        () => SpinWait.SpinUntil(() => this.AsyncHandlerCalls >= numberOfCalls && this.HandlerCalls >= numberOfCalls))
                        .ConfigureAwait(false);
            }
        }
 
    }
}