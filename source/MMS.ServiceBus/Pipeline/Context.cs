//-------------------------------------------------------------------------------
// <copyright file="Context.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The context is a key value bag which allows typed retrieval of values.
    /// </summary>
    /// <remarks>I'm not sure if having a dictionary internally is really justified. In theory this allows to snapshot also values internally 
    /// in the context without putting the burden into the caller.</remarks>
    public abstract class Context
    {
        private readonly IDictionary<string, object> stash = new Dictionary<string, object>();

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