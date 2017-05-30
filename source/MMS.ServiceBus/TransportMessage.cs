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
    using System.Linq;
    using System.Threading.Tasks;
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
                { HeaderKeys.ScheduledEnqueueTimeUtc, null },
                { HeaderKeys.DelayedDeliveryCount, 0.ToString() },
            };
        }

        public TransportMessage(BrokeredMessage message)
        {
            this.Headers = new Dictionary<string, string>
            {
                { HeaderKeys.MessageId, message.MessageId },
                { HeaderKeys.CorrelationId, message.CorrelationId },
                { HeaderKeys.MessageType, message.ContentType },
                { HeaderKeys.ReplyTo, message.ReplyTo },
                { HeaderKeys.ScheduledEnqueueTimeUtc, message.ScheduledEnqueueTimeUtc.Ticks.ToString() }
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
            get { return (Queue)this.Headers[HeaderKeys.ReplyTo].Parse(); }
            set { this.Headers[HeaderKeys.ReplyTo] = value.ToString(); }
        }

        public virtual int DeliveryCount
        {
            get { return this.message != null ? this.message.DeliveryCount : 0; }
        }

        public IDictionary<string, string> Headers { get; private set; }

        public DateTime ScheduledEnqueueTimeUtc
        {
            get { return new DateTime(long.Parse(this.Headers[HeaderKeys.ScheduledEnqueueTimeUtc] ?? "0")); }
            set { this.Headers[HeaderKeys.ScheduledEnqueueTimeUtc] = value.Ticks.ToString(); }
        }

        public int DelayedDeliveryCount
        {
            get { return int.Parse(this.Headers[HeaderKeys.DelayedDeliveryCount] ?? "0"); }
            set { this.Headers[HeaderKeys.DelayedDeliveryCount] = value.ToString(); }
        }

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
                ReplyTo = this.ReplyTo != null ? this.ReplyTo.ToString() : null,
                ScheduledEnqueueTimeUtc = this.ScheduledEnqueueTimeUtc,
            };

            foreach (KeyValuePair<string, string> pair in this.Headers)
            {
                brokeredMessage.Properties.Add(pair.Key, pair.Value);
            }

            return brokeredMessage;
        }

        public Task DeadLetterAsync()
        {
            var deadLetterHeaders = this.Headers.Where(x => x.Key.StartsWith(HeaderKeys.FailurePrefix, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x)
                .ToDictionary(x => x.Key, x => (object)x.Value);

            return this.DeadLetterAsyncInternal(deadLetterHeaders);
        }

        protected virtual Task DeadLetterAsyncInternal(IDictionary<string, object> deadLetterHeaders)
        {
            return this.message.DeadLetterAsync(deadLetterHeaders);
        }
    }
}