//-------------------------------------------------------------------------------
// <copyright file="NewtonsoftJsonMessageSerializer.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    public class NewtonsoftJsonMessageSerializer : IMessageSerializer
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
            var writer = new JsonTextWriter(new StreamWriter(body));
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, message);

            body.Flush();
            body.Position = 0;
        }

        public object Deserialize(Stream body, Type messageType)
        {
            var reader = new JsonTextReader(new StreamReader(body));
            var serializer = new JsonSerializer();
            return serializer.Deserialize(reader, messageType);
        }
    }
}