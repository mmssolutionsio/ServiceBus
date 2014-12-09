//-------------------------------------------------------------------------------
// <copyright file="DeserializeTransportMessageStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    public class DeserializeTransportMessageStep : IIncomingTransportStep
    {
        private readonly IMessageSerializer serializer;

        private readonly LogicalMessageFactory factory;

        public DeserializeTransportMessageStep(IMessageSerializer serializer)
        {
            this.factory = new LogicalMessageFactory();
            this.serializer = serializer;
        }

        public Task Invoke(IncomingTransportContext context, IBusForHandler bus, Func<Task> next)
        {
            var transportMessage = context.TransportMessage;

            try
            {
                context.Set(this.Extract(transportMessage));
            }
            catch (Exception exception)
            {
                throw new SerializationException(string.Format("An error occurred while attempting to extract logical messages from transport message {0}", transportMessage), exception);
            }

            return next();
        }

        private LogicalMessage Extract(TransportMessage transportMessage)
        {
            Type messageType = Type.GetType(transportMessage.MessageType, true, true);

            object message = this.serializer.Deserialize(transportMessage.Body, messageType);

            return this.factory.Create(messageType, message, transportMessage.Headers);
        }
    }
}