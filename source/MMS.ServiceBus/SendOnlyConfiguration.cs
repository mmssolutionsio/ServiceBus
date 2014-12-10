//-------------------------------------------------------------------------------
// <copyright file="SendOnlyConfiguration.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    public class SendOnlyConfiguration : EndpointConfiguration
    {
        public SendOnlyConfiguration()
        {
            this.Endpoint("SendOnly");
        }
    }
}