//-------------------------------------------------------------------------------
// <copyright file="BrokerExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Testing
{
    public static class BrokerExtensions
    {
        public static void Start(this Broker broker)
        {
            broker.StartAsync().Wait();
        }

        public static void Stop(this Broker broker)
        {
            broker.StopAsync().Wait();
        }
    }
}