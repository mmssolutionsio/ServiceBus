//-------------------------------------------------------------------------------
// <copyright file="IBusForHandler.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBusForHandler : IBus
    {
        Task Reply(object message);

        IDictionary<string, string> Headers(object message);

        void DoNotContinueDispatchingCurrentMessageToHandlers();
    }
}