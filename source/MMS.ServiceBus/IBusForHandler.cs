//-------------------------------------------------------------------------------
// <copyright file="IBusForHandler.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBusForHandler : IBus
    {
        Task Reply(object message, ReplyOptions options = null);

        IDictionary<string, string> Headers(object message);

        void DoNotContinueDispatchingCurrentMessageToHandlers();

        /// <summary>
        /// Defers the message back to the current bus.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="scheduledEnqueueTimeUtc">scheduled time to enqueue message in UTC</param>
        Task Postpone(object message, DateTime scheduledEnqueueTimeUtc);
    }
}