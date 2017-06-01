// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostponingMessages.cs" company="MMS AG">
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
    using System.Threading.Tasks;
    using FluentAssertions;
    using NUnit.Framework;
    using Pipeline;
    using Testing;

    [TestFixture]
    public class PostponingMessages
    {
        private const string SenderEndpointName = "Sender";
        private const string ReceiverEndpointName = "Receiver";

        private static readonly DateTime ScheduledEnqueTimeUtc = DateTime.Now + 2.Hours();
        private Context context;

        private PostponingHandlerRegistrySimulator registry;

        private Broker broker;
        private MessageUnit sender;
        private MessageUnit receiver;

        [SetUp]
        public void SetUp()
        {
            this.context = new Context();

            this.registry = new PostponingHandlerRegistrySimulator(this.context);

            this.broker = new Broker();
            this.sender = new MessageUnit(new EndpointConfiguration().Endpoint(SenderEndpointName).Concurrency(1))
                .Use(new AlwaysRouteToDestination(Queue.Create(ReceiverEndpointName)))
                .Use(this.registry);

            this.receiver = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverEndpointName).Concurrency(1))
                .Use(new AlwaysRouteToDestination(Queue.Create(SenderEndpointName)))
                .Use(this.registry);

            this.broker
                .Register(this.sender)
                .Register(this.receiver);

            this.broker.Start();
        }

        [Test]
        public async Task WhenMessageHandlerPostponesMessage_PostponedMessageShouldBeDeliveredToRegisteredHandlers()
        {
            var sendOptions = new SendOptions();
            
            await this.sender.Send(new Message(), sendOptions);

            this.context.PostponedHandlerCalls.Should().Be(2);
        }

        [Test]
        public async Task WhenMessageHandlerPostponesMessage_PostponedTransportMessageContainsScheduledEnqueueTimeUtc()
        {
            var sendOptions = new SendOptions { ScheduledEnqueueTimeUtc = ScheduledEnqueTimeUtc };
            
            await this.sender.Send(new Message(), sendOptions);

            this.receiver.OutgoingTransport.Should().OnlyContain(msg => msg.ScheduledEnqueueTimeUtc == ScheduledEnqueTimeUtc);
        }

        [Test]
        public async Task WhenMessageHandlerPostponesMessage_PostponedMessageContainsOriginalMessageUserSetCorrelationId()
        {
            var sendOptions = new SendOptions { CorrelationId = "12351435" };

            await this.sender.Send(new Message(), sendOptions);

            this.receiver.OutgoingTransport.Should().OnlyContain(msg => msg.Headers[HeaderKeys.CorrelationId] == sendOptions.CorrelationId);
        }

        [Test]
        public async Task WhenMessageHandlerPostponesMessage_PostponedMessageContainsOriginalMessageGeneratedCorrelationId()
        {
            var sendOptions = new SendOptions();
            
            await this.sender.Send(new Message(), sendOptions);

            var expectedCorrelationId = this.receiver.IncomingTransport[0].CorrelationId;
            this.receiver.OutgoingTransport.Should().OnlyContain(msg => msg.Headers[HeaderKeys.CorrelationId] == expectedCorrelationId);
            this.receiver.OutgoingTransport.Should().OnlyContain(msg => msg.CorrelationId == expectedCorrelationId);
        }

        [Test]
        public async Task WhenMessageHandlerPostponesMessage_PostponedMessageContainsOriginalMessageReplyTo()
        {
            var sendOptions = new SendOptions();
            
            await this.sender.Send(new Message(), sendOptions);

            var incomingMessageReplyTo = this.receiver.IncomingTransport[0].ReplyTo.ToString();
            var shortReplyTo = RemoveBrackets(incomingMessageReplyTo);
            this.receiver.OutgoingTransport.Should().OnlyContain(msg => msg.ReplyTo.ToString() == incomingMessageReplyTo);
            this.receiver.OutgoingTransport.Should().OnlyContain(msg => msg.Headers[HeaderKeys.ReplyTo] == shortReplyTo);
        }

        [Test]
        public async Task WhenMessageHandlerPostponesMessage_PostponedMessageContainsOriginalMessageCustomHeader()
        {
            var sendOptions = new SendOptions();
            const string MyOwnHeaderKey = "MyOwnHeader";
            const string MyOwnHeaderValue = "Bla";
            sendOptions.Headers.Add(new KeyValuePair<string, string>(MyOwnHeaderKey, MyOwnHeaderValue));
            
            await this.sender.Send(new Message(), sendOptions);

            this.receiver.OutgoingTransport.Should().OnlyContain(msg => msg.Headers[MyOwnHeaderKey] == MyOwnHeaderValue);
        }

        [Test]
        public async Task WhenMessageHandlerPostponesMessage_PostponedMessageHasAnotherMessageIdThanOriginalMessage()
        {
            var sendOptions = new SendOptions();

            await this.sender.Send(new Message(), sendOptions);

            var incomingMessageId = this.receiver.IncomingTransport[0].Id;
            this.receiver.OutgoingTransport.Should().OnlyContain(msg => msg.Id != incomingMessageId);
            this.receiver.OutgoingTransport.Should().OnlyContain(msg => msg.Headers[HeaderKeys.MessageId] != incomingMessageId);
        }

        [TearDown]
        public void TearDown()
        {
            this.broker.Stop();
        }

        private static string RemoveBrackets(string txt)
        {
            return txt.Replace("{", string.Empty).Replace("}", string.Empty);
        }

        private class PostponingHandlerRegistrySimulator : HandlerRegistry
        {
            private readonly Context context;

            public PostponingHandlerRegistrySimulator(Context context)
            {
                this.context = context;
            }

            public override IReadOnlyCollection<object> GetHandlers(Type messageType)
            {
                if (messageType == typeof(Message))
                {
                    return this.ConsumeWith(
                        new AsyncMessageHandler(this.context),
                        new PostponingMessageHandler(this.context).AsAsync());
                }

                if (messageType == typeof(PostponedMessage))
                {
                    return this.ConsumeWith(new PostponedMessageHandler(this.context));
                }

                return this.ConsumeAll();
            }
        }

        public class PostponedMessageHandler : IHandleMessageAsync<PostponedMessage>
        {
            private readonly Context context;

            public PostponedMessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(PostponedMessage message, IBusForHandler bus)
            {
                this.context.PostponedHandlerCalls += 1;
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
                await bus.Postpone(new PostponedMessage(), ScheduledEnqueTimeUtc);
            }
        }

        public class PostponingMessageHandler : IHandleMessage<Message>
        {
            private readonly Context context;

            public PostponingMessageHandler(Context context)
            {
                this.context = context;
            }

            public void Handle(Message message, IBusForHandler bus)
            {
                bus.Postpone(new PostponedMessage(), ScheduledEnqueTimeUtc).Wait();
            }
        }

        public class Message
        {
        }

        public class PostponedMessage
        {
        }

        public class Context
        {
            public int PostponedHandlerCalls { get; set; }
        }
    }
}