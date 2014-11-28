using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public class DeserializeTransportMessagePipelineStep : IIncomingTransportPipelineStep
    {
        private readonly IMessageSerializer serializer;

        private readonly LogicalMessageFactory factory;

        public DeserializeTransportMessagePipelineStep(IMessageSerializer serializer, LogicalMessageFactory factory)
        {
            this.factory = factory;
            this.serializer = serializer;
        }

        public async Task Invoke(IncomingTransportContext context, IBus bus, Func<Task> next)
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

            await next();
        }

        private LogicalMessage Extract(TransportMessage transportMessage)
        {
            var messageType = Type.GetType(transportMessage.Headers[HeaderKeys.MessageType], true);

            object message = this.serializer.Deserialize(transportMessage.Body, messageType);

            return this.factory.Create(messageType, message, transportMessage.Headers);
        }
    }
}