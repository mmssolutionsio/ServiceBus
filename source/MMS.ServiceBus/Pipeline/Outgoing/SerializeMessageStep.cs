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

                context.OutgoingTransportMessage.ContentType = this.serializer.ContentType;
                context.OutgoingTransportMessage.MessageType = context.LogicalMessage.Instance.GetType().AssemblyQualifiedName;

                context.OutgoingTransportMessage.SetBody(ms);

                await next()
                    .ConfigureAwait(false);
            }
        }
    }
}