//-------------------------------------------------------------------------------
// <copyright file="AsyncClosable.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Threading.Tasks;

    public class AsyncClosable
    {
        private readonly Func<Task> closable;

        public AsyncClosable(Func<Task> closable)
        {
            this.closable = closable;
        }

        public Task CloseAsync()
        {
            return this.closable();
        }
    }
}