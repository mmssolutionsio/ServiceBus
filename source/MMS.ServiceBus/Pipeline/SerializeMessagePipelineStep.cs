using System;
using System.IO;
using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public class SerializeMessagePipelineStep : IOutgoingTransportPipelineStep
    {
        private readonly IMessageSerializer serializer;

        public SerializeMessagePipelineStep(IMessageSerializer serializer)
        {
            this.serializer = serializer;
        }

        public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            using (var ms = new MemoryStream())
            {
                this.serializer.Serialize(context.LogicalMessage.Instance, ms);

                context.TransportMessage.Headers[HeaderKeys.ContentType] = this.serializer.ContentType;
                context.TransportMessage.Headers[HeaderKeys.MessageType] = context.LogicalMessage.Instance.GetType().AssemblyQualifiedName;

                context.TransportMessage.SetBody(ms);

                foreach (var headerEntry in context.LogicalMessage.Headers)
                {
                    context.TransportMessage.Headers[headerEntry.Key] = headerEntry.Value;
                }

                await next();
            }
        }
    }
}