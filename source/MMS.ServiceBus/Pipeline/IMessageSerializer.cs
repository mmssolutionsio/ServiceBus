namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    using System;
    using System.IO;

    public interface IMessageSerializer
    {
        string ContentType { get; }

        void Serialize(object message, Stream body);

        object Deserialize(Stream body, Type messageType);
    }
}