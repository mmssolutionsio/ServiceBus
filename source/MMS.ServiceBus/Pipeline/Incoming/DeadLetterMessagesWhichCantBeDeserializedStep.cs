//-------------------------------------------------------------------------------
// <copyright file="DeadLetterMessagesWhichCantBeDeserializedStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    public class DeadLetterMessagesWhichCantBeDeserializedStep : IIncomingTransportStep
    {
        public async Task Invoke(IncomingTransportContext context, IBusForHandler bus, Func<Task> next)
        {
            ExceptionDispatchInfo serializationException = null;
            try
            {
                await next()
                    .ConfigureAwait(false);
            }
            catch (SerializationException exception)
            {
                // We can't do async in a catch block, therefore we have to capture the exception!
                serializationException = ExceptionDispatchInfo.Capture(exception);
            }

            if (SerializationExceptionHasBeenCaught(serializationException))
            {
                var message = context.TransportMessage;

// ReSharper disable PossibleNullReferenceException
                message.SetFailureHeaders(serializationException.SourceException, "Messages which can't be deserialized are deadlettered immediately");
// ReSharper restore PossibleNullReferenceException
                await message.DeadLetterAsync()
                    .ConfigureAwait(false);

                // Because we instructed the message to deadletter it is safe to rethrow. The broker will not redeliver.
                serializationException.Throw();
            }
        }

        private static bool SerializationExceptionHasBeenCaught(ExceptionDispatchInfo serializationException)
        {
            return serializationException != null;
        }
    }
}