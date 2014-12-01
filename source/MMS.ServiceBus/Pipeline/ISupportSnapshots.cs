//-------------------------------------------------------------------------------
// <copyright file="ISupportSnapshots.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    public interface ISupportSnapshots
    {
        void TakeSnapshot();

        void DeleteSnapshot();
    }
}