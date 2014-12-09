//-------------------------------------------------------------------------------
// <copyright file="MessageSenderSendOptionsExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    public static class MessageSenderSendOptionsExtensions
    {
        public static string Destination(this SendOptions options)
        {
            return options.Queue.Destination;
        }
    }
}