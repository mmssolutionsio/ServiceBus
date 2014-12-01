//-------------------------------------------------------------------------------
// <copyright file="EndpointConfigurationExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    internal static class EndpointConfigurationExtensions
    {
        public static QueueDescription Configure(this EndpointConfiguration configuration, Queue queue)
        {
            return new QueueDescription(queue);
        }

        public static MessageReceiver Configure(this EndpointConfiguration configuration, MessageReceiver receiver)
        {
            receiver.PrefetchCount = configuration.PrefetchCount;
            return receiver;
        }

        public static OnMessageOptions Options(this EndpointConfiguration configuration)
        {
            return new OnMessageOptions { AutoComplete = true, MaxConcurrentCalls = configuration.MaxConcurrency };
        }
    }
}