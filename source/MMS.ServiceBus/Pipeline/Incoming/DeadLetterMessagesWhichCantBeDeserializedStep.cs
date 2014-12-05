//-------------------------------------------------------------------------------
// <copyright file="DeadLetterMessagesWhichCantBeDeserializedStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    public class DeadLetterMessagesWhichCantBeDeserializedStep : IIncomingTransportStep
    {
        public async Task Invoke(IncomingTransportContext context, IBus bus, Func<Task> next)
        {
            SerializationException serializationException = null;
            try
            {
                await next()
                    .ConfigureAwait(false);
            }
            catch (SerializationException exception)
            {
                // We can't do async in a catch block, therefore we have to capture the exception!
                serializationException = exception;
            }

            if (SerializationExceptionHasBeenCaught(serializationException))
            {
                var message = context.TransportMessage;

// ReSharper disable PossibleNullReferenceException
                message.Headers[HeaderKeys.DeadLetterReason] = serializationException.Message;
// ReSharper restore PossibleNullReferenceException
                message.Headers[HeaderKeys.DeadLetterDescription] = "Messages which can't be deserialized are deadlettered immediately";

                await message.DeadLetterAsync()
                    .ConfigureAwait(false);
            }
        }

        private static bool SerializationExceptionHasBeenCaught(SerializationException serializationException)
        {
            return serializationException != null;
        }
    }
}