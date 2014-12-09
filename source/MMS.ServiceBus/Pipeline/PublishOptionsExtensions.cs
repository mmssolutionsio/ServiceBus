//-------------------------------------------------------------------------------
// <copyright file="PublishOptionsExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    public static class PublishOptionsExtensions
    {
        public static string Destination(this PublishOptions options)
        {
            return options.Topic.Destination;
        }
    }
}