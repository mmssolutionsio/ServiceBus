//-------------------------------------------------------------------------------
// <copyright file="TransportMessage.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.ServiceBus.Messaging;

    public class TransportMessage
    {
        private readonly BrokeredMessage message;

        private Stream body;

        public TransportMessage()
        {
            var id = Guid.NewGuid().ToString();

            this.Headers = new Dictionary<string, string>
            {
                { HeaderKeys.MessageId, id },
                { HeaderKeys.CorrelationId, id },
                { HeaderKeys.ContentType, null },
                { HeaderKeys.ReplyTo, null },
            };
        }

        public TransportMessage(BrokeredMessage message)
        {
            this.Headers = new Dictionary<string, string>
            {
                { HeaderKeys.MessageId, message.MessageId },
                { HeaderKeys.CorrelationId, message.CorrelationId },
                { HeaderKeys.ContentType, message.ContentType },
                { HeaderKeys.ReplyTo, message.ReplyTo }
            };

            this.message = message;

            foreach (var pair in message.Properties)
            {
                if (!this.Headers.ContainsKey(pair.Key))
                {
                    this.Headers.Add(pair.Key, (string)pair.Value);
                }
            }
        }

        public string Id
        {
            get { return this.Headers[HeaderKeys.MessageId]; }
        }

        public string CorrelationId
        {
            get { return this.Headers[HeaderKeys.CorrelationId]; }
        }

        public string ContentType
        {
            get { return this.Headers[HeaderKeys.ContentType]; }
        }

        public Address ReplyTo
        {
            get { return Address.Parse(this.Headers[HeaderKeys.ReplyTo]); }
        }

        public IDictionary<string, string> Headers { get; private set; }

        public Stream Body
        {
            get { return this.body ?? (this.body = this.message.GetBody<Stream>()); }
        }

        public void SetBody(Stream body)
        {
            if (this.body != null)
            {
                throw new InvalidOperationException("Body is already set.");
            }

            this.body = body;
        }

        public BrokeredMessage ToBrokeredMessage()
        {
            var brokeredMessage = new BrokeredMessage(this.body, false)
            {
                ContentType = this.ContentType, 
                MessageId = this.Id, 
                CorrelationId = this.CorrelationId,
                ReplyTo = this.ReplyTo,
            };

            foreach (KeyValuePair<string, string> pair in this.Headers)
            {
                brokeredMessage.Properties.Add(pair.Key, pair.Value);
            }

            return brokeredMessage;
        }

        public void Complete()
        {
            this.message.Complete();
        }

        public void Abandon()
        {
            this.message.Abandon();
        }
    }
}