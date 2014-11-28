using System;
using System.IO;

namespace MMS.ServiceBus.Pipeline
{
    public interface IMessageSerializer
    {
        string ContentType { get; }

        void Serialize(object message, Stream body);

        object Deserialize(Stream body, Type messageType);
    }
}