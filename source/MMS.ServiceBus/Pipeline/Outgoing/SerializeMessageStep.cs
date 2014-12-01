//-------------------------------------------------------------------------------
// <copyright file="SerializeMessageStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class SerializeMessageStep : IOutgoingTransportStep
    {
        private readonly IMessageSerializer serializer;

        public SerializeMessageStep(IMessageSerializer serializer)
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