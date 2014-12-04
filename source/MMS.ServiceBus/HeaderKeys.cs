//-------------------------------------------------------------------------------
// <copyright file="HeaderKeys.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    public static class HeaderKeys
    {
        public const string ContentType = "MMS.ServiceBus.ContentType";

        public const string MessageType = "MMS.ServiceBus.MessageType";

        public const string MessageId = "MMS.ServiceBus.MessageId";

        public const string CorrelationId = "MMS.ServiceBus.CorrelationId";

        public const string ReplyTo = "MMS.ServiceBus.ReplyTo";

        public const string MessageIntent = "MMS.ServiceBus.MessageIntent";

        public const string DeadLetterReason = "MMS.ServiceBus.DeadLetterReason";

        public const string DeadLetterDescription = "MMS.ServiceBus.DeadLetterDescription";
    }
}