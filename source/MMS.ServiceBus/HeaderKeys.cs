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

        public const string FailurePrefix = "MMS.ServiceBus.Failure.";

        public const string DeadLetterReason = FailurePrefix + "Reason";
        public const string ExceptionReason = FailurePrefix + "Exception.Reason";
        public const string ExceptionType = FailurePrefix + "Exception.ExceptionType";
        public const string InnerExceptionType = FailurePrefix + "Exception.InnerExceptionType";
        public const string ExceptionHelpLink = FailurePrefix + "Exception.HelpLink";
        public const string ExceptionMessage = FailurePrefix + "Exception.Message";
        public const string ExceptionSource = FailurePrefix + "Exception.Source";
        public const string ExceptionStacktrace = FailurePrefix + "Exception.StackTrace";
        public const string TimeOfFailure = FailurePrefix + "TimeOfFailure";
    }
}