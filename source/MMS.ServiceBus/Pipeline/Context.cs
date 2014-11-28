using System;
using System.Collections.Generic;

namespace MMS.ServiceBus.Pipeline
{
    public abstract class Context
    {
        private readonly IDictionary<string, object> stash = new Dictionary<string, object>();

        private ISupportSnapshots chain;

        public T Get<T>()
        {
            return this.Get<T>(typeof(T).FullName);
        }

        public T Get<T>(string key)
        {
            object result;

            if (!this.stash.TryGetValue(key, out result))
            {
                throw new KeyNotFoundException("No item found in behavior context with key: " + key);
            }

            return (T)result;
        }

        public void Set<T>(T t)
        {
            this.Set(typeof(T).FullName, t);
        }

        public void Set<T>(string key, T t)
        {
            this.stash[key] = t;
        }

        internal void SetChain(ISupportSnapshots chain)
        {
            this.chain = chain;
        }

        internal IDisposable CreateSnapshot()
        {
            return new SnapshotRegion(this.chain);
        }
    }
}