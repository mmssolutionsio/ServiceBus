namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using ServiceBus.Pipeline;

    [TestFixture]
    public class SendingMessagesIntegration
    {
        private IntegrationMessageCentral central;

        private Context context;

        private HandlerRegistrySimulator registry;

        private IDisposable contextDispose;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            this.registry = new HandlerRegistrySimulator();

            this.central = new IntegrationMessageCentral(this.registry);
            this.central.StartAsync().Wait();
        }

        [SetUp]
        public void SetUp()
        {
            this.context = new Context();

            this.contextDispose = this.registry.SetContext(this.context);
        }

        [Test]
        public async Task WhenOneMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.central.Sender.Send(new Message { Bar = 42 });

            await Task.Delay(100);

            await this.context.Wait(1);

            Assert.AreEqual(1, this.context.AsyncHandlerCalls);
            Assert.AreEqual(1, this.context.HandlerCalls);
        }

        [Test]
        public async Task WhenMultipeMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await this.central.Sender.Send(new Message { Bar = 42 });
            await this.central.Sender.Send(new Message { Bar = 43 });
            await this.central.Sender.Send(new Message { Bar = 44 });
            await this.central.Sender.Send(new Message { Bar = 45 });

            await Task.Delay(100);

            await this.context.Wait(4);

            Assert.AreEqual(4, this.context.AsyncHandlerCalls);
            Assert.AreEqual(4, this.context.HandlerCalls);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            this.central.StopAsync().Wait();
        }

        [TearDown]
        public void TearDown()
        {
            this.contextDispose.Dispose();
        }

        public class HandlerRegistrySimulator : HandlerRegistry
        {
            private Context context;

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

            public IDisposable SetContext(Context context)
            {
                this.context = context;

                return new Disposable(this);
            }

            private class Disposable : IDisposable
            {
                private readonly HandlerRegistrySimulator registry;

                public Disposable(HandlerRegistrySimulator registry)
                {
                    this.registry = registry;
                }

                public void Dispose()
                {
                    this.registry.context = new Context();
                }
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
                await Task.Delay(20);
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