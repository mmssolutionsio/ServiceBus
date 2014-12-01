//-------------------------------------------------------------------------------
// <copyright file="DataContractMessageSerializer.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Json;

    public class DataContractMessageSerializer : IMessageSerializer
    {
        public string ContentType
        {
            get
            {
                return "application/json";
            }
        }

        public void Serialize(object message, Stream body)
        {
            var serializer = new DataContractJsonSerializer(message.GetType());
            serializer.WriteObject(body, message);

            body.Flush();
            body.Position = 0;
        }

        public object Deserialize(Stream body, Type messageType)
        {
            var serializer = new DataContractJsonSerializer(messageType);
            return serializer.ReadObject(body);
        }
    }
}