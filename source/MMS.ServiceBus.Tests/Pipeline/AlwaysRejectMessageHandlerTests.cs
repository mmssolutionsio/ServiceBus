//-------------------------------------------------------------------------------
// <copyright file="AlwaysRejectMessageHandlerTests.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AlwaysRejectMessageHandlerTests
    {
        private AlwaysRejectMessageHandler testee;

        [SetUp]
        public void SetUp()
        {
            this.testee = new AlwaysRejectMessageHandler();
        }

        [Test]
        public void Handle_Throws()
        {
            var bus = new Mock<IBusForHandler>();
            bus.Setup(x => x.Headers(It.IsAny<object>())).Returns(new Dictionary<string, string>
            {
                {HeaderKeys.MessageId, Guid.NewGuid().ToString()}
            });

            var action = new Func<Task>(async () => await this.testee.Handle(new object(), bus.Object));

            action.ShouldThrow<InvalidOperationException>();
        }
    }
}