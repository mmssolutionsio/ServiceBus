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
            try
            {
                await next()
                    .ConfigureAwait(false);
            }
            catch (SerializationException exception)
            {
                var message = context.TransportMessage;

                message.Headers[HeaderKeys.DeadLetterReason] = exception.Message;
                message.Headers[HeaderKeys.DeadLetterDescription] = "Messages which can't be deserialized are deadlettered immediately";
                
                // We can't do async in a catch block!
                context.TransportMessage.DeadLetter();
            }
        }
    }
}