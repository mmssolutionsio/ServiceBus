//-------------------------------------------------------------------------------
// <copyright file="Context.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The context is a key value bag which allows typed retrieval of values.
    /// </summary>
    public abstract class Context : ISupportSnapshots
    {
        private readonly Stack<IDictionary<string, Entry>> snapshots = new Stack<IDictionary<string, Entry>>();

        private readonly IDictionary<string, Entry> stash = new Dictionary<string, Entry>();

        private ISupportSnapshots chain;

        protected Context(EndpointConfiguration.ReadOnly configuration)
        {
            this.Set(configuration);
        }

        public EndpointConfiguration.ReadOnly Configuration
        {
            get { return this.Get<EndpointConfiguration.ReadOnly>(); }
        }

        public T Get<T>()
        {
            return this.Get<T>(typeof(T).FullName);
        }

        public T Get<T>(string key)
        {
            Entry result;

            if (!this.stash.TryGetValue(key, out result))
            {
                throw new KeyNotFoundException("No item found in behavior context with key: " + key);
            }

            return (T)result.Value;
        }

        public void Set<T>(T t, ShouldBeSnapshotted candidateForSnapshot = ShouldBeSnapshotted.No)
        {
            this.Set(typeof(T).FullName, t, candidateForSnapshot);
        }

        public void Set<T>(string key, T t, ShouldBeSnapshotted candidateForSnapshot = ShouldBeSnapshotted.No)
        {
            this.stash[key] = new Entry(t, candidateForSnapshot);
        }

        public void TakeSnapshot()
        {
            this.snapshots.Push(this.stash.Where(x => x.Value.CandidateForSnapshot == ShouldBeSnapshotted.Yes).ToDictionary(k => k.Key, v => v.Value));
        }

        public void DeleteSnapshot()
        {
            IDictionary<string, Entry> allSnapshottedCandidates = this.snapshots.Pop();

            foreach (var allSnapshottedCandidate in allSnapshottedCandidates)
            {
                this.stash[allSnapshottedCandidate.Key] = allSnapshottedCandidate.Value;
            }
        }

        internal void SetChain(ISupportSnapshots chain)
        {
            this.chain = chain;
        }

        internal IDisposable CreateSnapshot()
        {
            return new SnapshotRegion(this.chain, this);
        }

        private class Entry
        {
            public Entry(object value, ShouldBeSnapshotted candidateForSnapshot = ShouldBeSnapshotted.No)
            {
                this.CandidateForSnapshot = candidateForSnapshot;
                this.Value = value;
            }

            public ShouldBeSnapshotted CandidateForSnapshot { get; private set; }

            public object Value { get; private set; }
        }
    }
}