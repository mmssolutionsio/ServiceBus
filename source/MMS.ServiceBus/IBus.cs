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
        Task Send(object message, SendOptions options = null);

        Task Publish(object message, PublishOptions options = null);

        void DoNotContinueDispatchingCurrentMessageToHandlers();
    }
}