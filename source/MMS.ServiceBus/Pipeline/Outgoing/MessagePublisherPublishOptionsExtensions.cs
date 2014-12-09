//-------------------------------------------------------------------------------
// <copyright file="MessagePublisherPublishOptionsExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    public static class MessagePublisherPublishOptionsExtensions
    {
        public static string Destination(this PublishOptions options)
        {
            return options.Topic.Destination;
        }
    }
}