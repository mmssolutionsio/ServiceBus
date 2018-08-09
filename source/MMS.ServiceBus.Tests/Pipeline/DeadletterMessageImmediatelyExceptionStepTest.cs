namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Incoming;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeadletterMessageImmediatelyExceptionStepTest
    {
        private DeadletterMessageImmediatelyExceptionStep testee;

        private Func<Task> pipelineStepRaisingException;

        private Func<Task> pipelineStepRaisingDeadletterMessageImmediatelyException;

        private Mock<IBusForHandler> busMock;

        private TestTransportMessage testTransportMessage;

        private IncomingLogicalContext incomingLogicalContext;

        private Exception internalException;

        [SetUp]
        public void Setup()
        {
            try
            {
                throw new InvalidOperationException("Internal test exception");
            }
            catch (Exception exc)
            {
                this.internalException = exc;
            }

            this.pipelineStepRaisingException = async () =>
            {
                await Task.Run(() =>
                    {
                        throw new InvalidOperationException();
                    });
            };

            this.pipelineStepRaisingDeadletterMessageImmediatelyException = async () =>
            {
                await Task.Run(() =>
                    {
                        throw new DeadletterMessageImmediatelyException(this.internalException);
                    });
            };

            this.testTransportMessage = new TestTransportMessage(typeof(Message).AssemblyQualifiedName);
            var readOnlyConfiguration = new EndpointConfiguration.ReadOnly(new EndpointConfiguration().MaximumImmediateRetryCount(10).MaximumDelayedRetryCount(10));
            var logicalMessage = new LogicalMessage(typeof(Message), new Message(), null);
            this.incomingLogicalContext = new IncomingLogicalContext(logicalMessage, this.testTransportMessage, readOnlyConfiguration);
            this.busMock = new Mock<IBusForHandler>();

            this.testee = new DeadletterMessageImmediatelyExceptionStep();
        }

        [Test]
        public async Task Invoke_WhenNoExceptionFromPipeline_ThenContinue()
        {
            var nextCalled = false;
            await this.testee.Invoke(
                null,
                null,
                async () =>
                {
                    nextCalled = true;
                    await Task.FromResult(0);
                });

            nextCalled.Should().BeTrue();
        }

        [Test]
        public void Invoke_WhenNotDeadletterMessageImmediatelyException_ThenRethrow()
        {
            Func<Task> action = async () => await this.testee.Invoke(this.incomingLogicalContext, null, this.pipelineStepRaisingException);

            action.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void Invoke_WhenDeadletterMessageImmediatelyException_ThenRethrowException()
        {

            Func<Task> action = async () => await this.testee.Invoke(this.incomingLogicalContext, this.busMock.Object, this.pipelineStepRaisingDeadletterMessageImmediatelyException);

            action.ShouldThrow<DeadletterMessageImmediatelyException>();
        }

        [Test]
        public void Invoke_WhenDeadletterMessageImmediatelyException_ThenDeadletterMessage()
        {
            Func<Task> action = async () => await this.testee.Invoke(this.incomingLogicalContext, this.busMock.Object, this.pipelineStepRaisingDeadletterMessageImmediatelyException);
            action.ShouldThrow<DeadletterMessageImmediatelyException>();

            var deadLetterHeaders = this.testTransportMessage.DeadLetterHeaders;

            deadLetterHeaders.Should().HaveCount(7);
            deadLetterHeaders[HeaderKeys.ExceptionReason].Should()
                .Be("Message is deadlettered immediately on DeadletterMessageImmediatelyException");
            deadLetterHeaders[HeaderKeys.ExceptionType].Should().Be(this.internalException.GetType().FullName);
            deadLetterHeaders[HeaderKeys.ExceptionMessage].Should().Be(this.internalException.Message);
            deadLetterHeaders[HeaderKeys.ExceptionSource].Should().Be(this.internalException.Source);
            deadLetterHeaders[HeaderKeys.ExceptionStacktrace].Should().Be(this.internalException.StackTrace);
        }

        public class TestTransportMessage : TransportMessage
        {
            public TestTransportMessage(string messageType)
            {
                this.MessageType = messageType;
            }

            public IDictionary<string, object> DeadLetterHeaders { get; private set; }

            protected override Task DeadLetterAsyncInternal(IDictionary<string, object> deadLetterHeaders)
            {
                this.DeadLetterHeaders = deadLetterHeaders;

                return Task.FromResult(0);
            }

        }

        public class Message
        {
            public int Bar { get; set; }
        }
    }
}