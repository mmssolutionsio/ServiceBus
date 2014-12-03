//-------------------------------------------------------------------------------
// <copyright file="IBus.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System.Threading.Tasks;
    using Pipeline;

    public interface IBus
    {
        /// <summary>
        /// Sends the message back to the current bus.
        /// </summary>
        /// <param name="message">The message to send.</param>
        Task SendLocal(object message);

        Task Send(object message, SendOptions options = null);

        Task Publish(object message, PublishOptions options = null);

        Task Reply(object message);

        void DoNotContinueDispatchingCurrentMessageToHandlers();
    }
}