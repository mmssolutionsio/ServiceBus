using System;

namespace MMS.ServiceBus.Pipeline
{
    public sealed class SnapshotRegion : IDisposable
    {
        private readonly ISupportSnapshots chain;

        public SnapshotRegion(ISupportSnapshots chain)
        {
            this.chain = chain;
            this.chain.TakeSnapshot();
        }

        public void Dispose()
        {
            this.chain.DeleteSnapshot();
        }
    }
}