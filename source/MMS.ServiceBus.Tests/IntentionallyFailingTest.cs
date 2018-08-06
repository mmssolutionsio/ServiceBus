using NUnit.Framework;

namespace MMS.ServiceBus
{
    using FluentAssertions;

    [TestFixture]
    public class IntentionallyFailingTest
    {
        [Test]
        public void FailsAlways()
        {
            true.Should().BeFalse();
        }
    }
}