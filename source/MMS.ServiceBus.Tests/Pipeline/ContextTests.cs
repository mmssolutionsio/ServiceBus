//-------------------------------------------------------------------------------
// <copyright file="ContextTests.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ContextTests
    {
        private TestableContext testee;

        [SetUp]
        public void SetUp()
        {
            this.testee = new TestableContext();
        }

        [Test]
        public void TakingASnapshot_SnapshotsChainAndSnapshotsCandidates()
        {
            const string SnapshotCandidiate = "SnapshotCandidiate";
            const string NotToBeSnapshoted = "NotToBeSnapshoted";

            var chain = new Chain();
            this.testee.SetChain(chain);

            this.testee.Set(SnapshotCandidiate, false, ShouldBeSnapshotted.Yes);
            this.testee.Set(NotToBeSnapshoted, false);

            using (this.testee.CreateSnapshot())
            {
                this.testee.Set(SnapshotCandidiate, true);
                this.testee.Set(NotToBeSnapshoted, true);
            }

            chain.SnapshotTaken.Should().BeTrue();
            chain.SnapshotDeleted.Should().BeTrue();
            this.testee.Get<bool>(SnapshotCandidiate).Should().BeFalse();
            this.testee.Get<bool>(NotToBeSnapshoted).Should().BeTrue();
        }

        private class TestableContext : Context
        {
            public TestableContext()
                : base(new SendOnlyConfiguration().Validate(), new ImmediateCompleteTransaction())
            {
            }
        }

        private class Chain : ISupportSnapshots
        {
            public bool SnapshotTaken { get; private set; }
            public bool SnapshotDeleted { get; private set; }

            public void TakeSnapshot()
            {
                this.SnapshotTaken = true;
            }

            public void DeleteSnapshot()
            {
                this.SnapshotDeleted = true;
            }
        }
    }
}