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
            var id = CombGuid.Generate().ToString();

            this.Headers = new Dictionary<string, string>
            {
                { HeaderKeys.MessageId, id },
                { HeaderKeys.CorrelationId, id },
                { HeaderKeys.ContentType, null },
                { HeaderKeys.ReplyTo, null },
                { HeaderKeys.MessageType, null },
                { HeaderKeys.MessageIntent, null },
            };
        }

        public TransportMessage(BrokeredMessage message)
        {
            this.Headers = new Dictionary<string, string>
            {
                { HeaderKeys.MessageId, message.MessageId },
                { HeaderKeys.CorrelationId, message.CorrelationId },
                { HeaderKeys.MessageType, message.ContentType },
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
            set { this.Headers[HeaderKeys.CorrelationId] = value; }
        }

        public string ContentType
        {
            get { return this.Headers[HeaderKeys.ContentType]; }
            set { this.Headers[HeaderKeys.ContentType] = value; }
        }

        public string MessageType
        {
            get { return this.Headers[HeaderKeys.MessageType]; }
            set { this.Headers[HeaderKeys.MessageType] = value; }
        }

        public MessageIntent MessageIntent
        {
            get
            {
                MessageIntent messageIntent;
                string messageIntentString = this.Headers[HeaderKeys.MessageIntent];
                Enum.TryParse(messageIntentString, true, out messageIntent);
                return messageIntent;
            }

            set
            {
                this.Headers[HeaderKeys.MessageIntent] = value.ToString();
            }
        }

        public Queue ReplyTo
        {
            get { return (Queue)Address.Parse(this.Headers[HeaderKeys.ReplyTo]); }
            set { this.Headers[HeaderKeys.ReplyTo] = value; }
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
                ContentType = this.MessageType,
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