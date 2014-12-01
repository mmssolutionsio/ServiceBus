//-------------------------------------------------------------------------------
// <copyright file="IMessageSerializer.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
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