//-------------------------------------------------------------------------------
// <copyright file="IBusForHandler.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBusForHandler : ITransactionalBusForHandlerProvider
    {
        Task SendLocal(object message);

        Task Send(object message, SendOptions options = null);

        Task Publish(object message, PublishOptions options = null);

        Task Reply(object message, ReplyOptions options = null);

        IDictionary<string, string> Headers(object message);

        void DoNotContinueDispatchingCurrentMessageToHandlers();
    }
}