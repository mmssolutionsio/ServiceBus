//-------------------------------------------------------------------------------
// <copyright file="SendOptionsExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    public static class SendOptionsExtensions
    {
        public static string Destination(this SendOptions options)
        {
            return options.Queue.Destination;
        }
    }
}