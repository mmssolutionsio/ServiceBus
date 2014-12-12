//-------------------------------------------------------------------------------
// <copyright file="SnapshotRegion.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;

    public sealed class SnapshotRegion : IDisposable
    {
        private readonly ISupportSnapshots[] chain;

        public SnapshotRegion(params ISupportSnapshots[] chain)
        {
            this.chain = chain;

            foreach (ISupportSnapshots snapshotable in this.chain)
            {
                snapshotable.TakeSnapshot();    
            }
        }

        public void Dispose()
        {
            foreach (ISupportSnapshots snapshotable in this.chain)
            {
                snapshotable.DeleteSnapshot();
            }
        }
    }
}