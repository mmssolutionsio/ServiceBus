//-------------------------------------------------------------------------------
// <copyright file="DeadletterMessageImmediatelyExceptionStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;

    public class DeadletterMessageImmediatelyExceptionStep : IIncomingLogicalStep
    {
        public async Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next)
        {
            ExceptionDispatchInfo exceptionDispatchInfo = null;
            try
            {
                await next()
                    .ConfigureAwait(false);
            }
            catch (DeadletterMessageImmediatelyException exception)
            {
                // We can't do async in a catch block, therefore we have to capture the exception!
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
            }

            if (HandleMessageCriticalExceptionHasBeenCaught(exceptionDispatchInfo))
            {
                var message = context.TransportMessage;

                // ReSharper disable PossibleNullReferenceException
                var exceptionToHeader = exceptionDispatchInfo.SourceException.InnerException ?? exceptionDispatchInfo.SourceException;
                message.SetFailureHeaders(exceptionToHeader, "Message is deadlettered immediately on DeadletterMessageImmediatelyException");
                // ReSharper restore PossibleNullReferenceException
                await message.DeadLetterAsync()
                    .ConfigureAwait(false);

                // Because we instructed the message to deadletter it is safe to rethrow. The broker will not redeliver.
                // Already deadlettered message should not be unlocked - throw for it
                exceptionDispatchInfo.Throw();
            }
        }

        private static bool HandleMessageCriticalExceptionHasBeenCaught(ExceptionDispatchInfo criticalException)
        {
            return criticalException != null;
        }
    }
}