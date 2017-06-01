// -------------------------------------------------------------------------------
//  <copyright file="DelayMessagesWhenImmediateRetryCountIsReachedStepTest.cs" company="MMS AG">
//    Copyright (c) MMS AG, 2008-2016
//  </copyright>
// -------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using FluentAssertions;

    using MMS.ServiceBus.Pipeline.Incoming;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DelayMessagesWhenImmediateRetryCountIsReachedStepTest
    {
        private DelayMessagesWhenImmediateRetryCountIsReachedStep testee;

        private static int MaxImmediateRetryCount = 5;

        private static int MaxDelayedRetryCount = 3;

        static int actualDeliveryCount;

        private Func<Task> pipelineStepRaisingException;

        private Mock<IBusForHandler> busMock;

        private TestTransportMessage testTransportMessage;

        private IncomingLogicalContext incomingLogicalContext;

        [SetUp]
        public void Setup()
        {
            this.pipelineStepRaisingException = () =>
            {
                throw new InvalidOperationException();
            };

            this.testTransportMessage = new TestTransportMessage(typeof(Message).AssemblyQualifiedName);
            var readOnlyConfiguration = new EndpointConfiguration.ReadOnly(new EndpointConfiguration().MaximumImmediateRetryCount(MaxImmediateRetryCount).MaximumDelayedRetryCount(MaxDelayedRetryCount));
            var logicalMessage = new LogicalMessage(typeof(Message), this.testTransportMessage, null);
            this.incomingLogicalContext = new IncomingLogicalContext(logicalMessage, this.testTransportMessage, readOnlyConfiguration);
            this.busMock = new Mock<IBusForHandler>();

            this.testee = new DelayMessagesWhenImmediateRetryCountIsReachedStep();
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
        public void Invoke_WhenExceptionFromPipelineAndNotMaxImmediateRetriesReached_ThenRethrow()
        {
            actualDeliveryCount = MaxImmediateRetryCount - 1;

            Func<Task> action = async () => await this.testee.Invoke(this.incomingLogicalContext, null, this.pipelineStepRaisingException);

            action.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void Invoke_WhenExceptionFromPipelineAndMaxImmediateRetriesReached_ThenConsumeOldMessage()
        {
            actualDeliveryCount = MaxImmediateRetryCount;

            Func<Task> action = async () => await this.testee.Invoke(this.incomingLogicalContext, this.busMock.Object, this.pipelineStepRaisingException);

            action.ShouldNotThrow<InvalidOperationException>();
        }

        [Test]
        public async Task Invoke_WhenExceptionFromPipelineAndMaxImmediateRetriesReachedButNotMaxDelayedRetryCountReached_ThenPostponeForDelayedRetryCountPowerOf2Second()
        {
            actualDeliveryCount = MaxImmediateRetryCount;

            for (int i = 0; i < MaxDelayedRetryCount; i++)
            {
                await this.testee.Invoke(this.incomingLogicalContext, this.busMock.Object, this.pipelineStepRaisingException);

                var expectedTimeSpan = TimeSpan.FromSeconds(Math.Pow(2, i));
                this.busMock.Verify(_ => _.Postpone(this.incomingLogicalContext.LogicalMessage.Instance, this.testee.Time + expectedTimeSpan));
            }
        }

        [Test]
        public void Invoke_WhenExceptionFromPipelineAndMaxDelayedRetriesReached_ThenRethrow()
        {
            actualDeliveryCount = MaxImmediateRetryCount;
            this.testTransportMessage.DelayedDeliveryCount = MaxDelayedRetryCount;
            Func<Task> action = async () => await this.testee.Invoke(this.incomingLogicalContext, this.busMock.Object, this.pipelineStepRaisingException);

            action.ShouldThrow<InvalidOperationException>();
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

            public override int DeliveryCount => actualDeliveryCount;

        }

        public class Message
        {
            public int Bar { get; set; }
        }
    }
}