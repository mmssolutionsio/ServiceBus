//-------------------------------------------------------------------------------
// <copyright file="IRenewLock.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System.Threading.Tasks;

    public interface IRenewLock
    {
        void RenewLock();

        Task RenewLockAsync();
    }
}