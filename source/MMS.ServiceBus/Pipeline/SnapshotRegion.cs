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