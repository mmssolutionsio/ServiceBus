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
            this.Id = Guid.NewGuid().ToString();
            this.CorrelationId = this.Id;

            this.Headers = new Dictionary<string, string>();
        }

        public TransportMessage(BrokeredMessage message)
        {
            this.Headers = new Dictionary<string, string>();
            this.message = message;

            this.Id = message.MessageId;
            this.CorrelationId = message.CorrelationId;

            foreach (var pair in message.Properties)
            {
                this.Headers.Add(pair.Key, (string) pair.Value);
            }
        }

        public string Id { get; set; }

        public string CorrelationId { get; set; }

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
                ContentType = this.Headers[HeaderKeys.MessageType], 
                MessageId = this.Id, 
                CorrelationId = this.CorrelationId
            };

            foreach (KeyValuePair<string, string> pair in this.Headers)
            {
                brokeredMessage.Properties.Add(pair.Key, pair.Value);
            }

            return brokeredMessage;
        }

        public virtual void Complete()
        {
            this.message.Complete();
        }

        public virtual void Abandon()
        {
            this.message.Abandon();
        }
    }
}