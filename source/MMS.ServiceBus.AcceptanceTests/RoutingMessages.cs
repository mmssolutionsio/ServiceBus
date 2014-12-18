//-------------------------------------------------------------------------------
// <copyright file="RoutingMessages.cs" company="Multimedia Solutions AG">
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
    using Pipeline.Outgoing;
    using ServiceBus.Pipeline;
    using ServiceBus.Testing;

    [TestFixture]
    public class RoutingMessages
    {
        private const string ReceiverOneEndpointName = "Receiver1";

        private const string ReceiverTwoEndpointName = "Receiver2";

        private const string ReceiverThreeEndpointName = "Receiver3";

        private Context context;

        private HandlerRegistrySimulator registry;

        private Broker broker;

        private MessageUnit sender;

        private MessageUnit receiverOne;

        private MessageUnit receiverTwo;

        private MessageUnit receiverThree;

        private Router router;

        [SetUp]
        public void SetUp()
        {
            this.context = new Context();
            this.registry = new HandlerRegistrySimulator(this.context);
            this.router = new Router();

            this.broker = new Broker();
            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint("Sender").Concurrency(1))
                .Use(this.router);
            this.receiverOne = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverOneEndpointName)
                .Concurrency(1)).Use(this.registry);
            this.receiverTwo = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverTwoEndpointName)
                .Concurrency(1)).Use(this.registry);
            this.receiverThree = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverThreeEndpointName)
                .Concurrency(1)).Use(this.registry);

            this.broker.Register(this.sender)
                       .Register(this.receiverOne)
                       .Register(this.receiverTwo)
                       .Register(this.receiverThree);

            this.broker.Start();
        }

        [TearDown]
        public void TearDown()
        {
            this.broker.Stop();
        }

        private class Router : IMessageRouter
        {
            public IReadOnlyCollection<Address> GetDestinationFor(Type messageType)
            {
                if (messageType == typeof(MessageForReceiverOne))
                {
                    return new ReadOnlyCollection<Address>(new List<Address> { Queue.Create(ReceiverOneEndpointName) });
                }

                if (messageType == typeof(MessageForReceiverTwo))
                {
                    return new ReadOnlyCollection<Address>(new List<Address> { Queue.Create(ReceiverTwoEndpointName) });
                }

                return new ReadOnlyCollection<Address>(new List<Address>());
            }
        }

        [Test]
        public async Task WhenSendingMessages_MessagesAreRoutedAccordingToTheMessageRouter()
        {
            await this.sender.Send(new MessageForReceiverOne { Bar = 42 });
            await this.sender.Send(new MessageForReceiverTwo { Bar = 42 });

            this.context.AsyncReceiverOneCalled.Should().BeInvokedOnce();
            this.context.SyncReceiverOneCalled.Should().BeInvokedOnce();
            this.context.AsyncReceiverTwoCalled.Should().BeInvokedOnce();
            this.context.SyncReceiverTwoCalled.Should().BeInvokedOnce();
        }

        [Test]
        public async Task WhenSendingMessages_WithSpecificSendDestination_MessagesAreRoutedByUserInput()
        {
            var sendOptions = new SendOptions { Queue = Queue.Create(ReceiverThreeEndpointName) };

            await this.sender.Send(new MessageForReceiverThree { Bar = 42 }, sendOptions);

            this.context.AsyncReceiverOneCalled.Should().NotBeInvoked();
            this.context.SyncReceiverOneCalled.Should().NotBeInvoked();
            this.context.AsyncReceiverTwoCalled.Should().NotBeInvoked();
            this.context.SyncReceiverTwoCalled.Should().NotBeInvoked();

            this.context.AsyncReceiverThreeCalled.Should().BeInvokedOnce();
            this.context.SyncReceiverThreeCalled.Should().BeInvokedOnce();
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
                if (messageType == typeof(MessageForReceiverOne))
                {
                    return this.ConsumeWith(
                        new AsyncMessageHandlerReceiverOne(this.context),
                        new MessageHandlerReceiverOne(this.context).AsAsync());
                }

                if (messageType == typeof(MessageForReceiverTwo))
                {
                    return this.ConsumeWith(
                        new AsyncMessageHandlerReceiverTwo(this.context),
                        new MessageHandlerReceiverTwo(this.context).AsAsync());
                }

                if (messageType == typeof(MessageForReceiverThree))
                {
                    return this.ConsumeWith(
                        new AsyncMessageHandlerReceiverThree(this.context),
                        new MessageHandlerReceiverThree(this.context).AsAsync());
                }

                return this.ConsumeAll();
            }
        }

        public class AsyncMessageHandlerReceiverOne : IHandleMessageAsync<MessageForReceiverOne>
        {
            private readonly Context context;

            public AsyncMessageHandlerReceiverOne(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverOne message, IBusForHandler bus)
            {
                this.context.AsyncReceiverOneCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class MessageHandlerReceiverOne : IHandleMessage<MessageForReceiverOne>
        {
            private readonly Context context;

            public MessageHandlerReceiverOne(Context context)
            {
                this.context = context;
            }

            public void Handle(MessageForReceiverOne message, IBusForHandler bus)
            {
                this.context.SyncReceiverOneCalled += 1;
            }
        }

        public class AsyncMessageHandlerReceiverTwo : IHandleMessageAsync<MessageForReceiverTwo>
        {
            private readonly Context context;

            public AsyncMessageHandlerReceiverTwo(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverTwo message, IBusForHandler bus)
            {
                this.context.AsyncReceiverTwoCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class MessageHandlerReceiverTwo : IHandleMessage<MessageForReceiverTwo>
        {
            private readonly Context context;

            public MessageHandlerReceiverTwo(Context context)
            {
                this.context = context;
            }

            public void Handle(MessageForReceiverTwo message, IBusForHandler bus)
            {
                this.context.SyncReceiverTwoCalled += 1;
            }
        }

        public class AsyncMessageHandlerReceiverThree : IHandleMessageAsync<MessageForReceiverThree>
        {
            private readonly Context context;

            public AsyncMessageHandlerReceiverThree(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverThree message, IBusForHandler bus)
            {
                this.context.AsyncReceiverThreeCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class MessageHandlerReceiverThree : IHandleMessage<MessageForReceiverThree>
        {
            private readonly Context context;

            public MessageHandlerReceiverThree(Context context)
            {
                this.context = context;
            }

            public void Handle(MessageForReceiverThree message, IBusForHandler bus)
            {
                this.context.SyncReceiverThreeCalled += 1;
            }
        }

        public class MessageForReceiverOne
        {
            public int Bar { get; set; }
        }

        public class MessageForReceiverTwo
        {
            public int Bar { get; set; }
        }

        public class MessageForReceiverThree
        {
            public int Bar { get; set; }
        }

        public class Context
        {
            public int AsyncReceiverOneCalled { get; set; }

            public int SyncReceiverOneCalled { get; set; }

            public int AsyncReceiverTwoCalled { get; set; }

            public int SyncReceiverTwoCalled { get; set; }

            public int AsyncReceiverThreeCalled { get; set; }

            public int SyncReceiverThreeCalled { get; set; }
        }
    }
}