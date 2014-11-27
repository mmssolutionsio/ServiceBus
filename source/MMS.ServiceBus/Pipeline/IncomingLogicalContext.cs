namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    using System;

    public class IncomingLogicalContext : Context
    {
        private const string HandlerInvocationAbortedKey = "HandlerInvocationAborted";

        public IncomingLogicalContext(LogicalMessage logicalMessage, TransportMessage message)
        {
            this.Set(logicalMessage);
            this.Set(message);
            this.Set(HandlerInvocationAbortedKey, false);
        }

        public LogicalMessage LogicalMessage
        {
            get
            {
                return this.Get<LogicalMessage>();
            }
        }

        public TransportMessage TransportMessage
        {
            get
            {
                return this.Get<TransportMessage>();
            }
        }

        public MessageHandler Handler
        {
            get
            {
                return this.Get<MessageHandler>();
            }

            set
            {
                this.Set(value);
            }
        }

        public bool HandlerInvocationAborted
        {
            get
            {
                return this.Get<bool>(HandlerInvocationAbortedKey);
            }

            set
            {
                this.Set<bool>(HandlerInvocationAbortedKey, value);
            }
        }

        //public IDisposable CreateSnapshot()
        //{
        //    var snapshot = new IncomingLogicalContextSnapshot
        //        {
        //            HandlerInvocationAborted = this.HandlerInvocationAborted
        //        };

        //    return new Disposable(() => { this.HandlerInvocationAborted = snapshot.HandlerInvocationAborted; });
        //}

        //private class IncomingLogicalContextSnapshot
        //{
        //    public bool HandlerInvocationAborted { get; set; }
        //}

        //private class Disposable : IDisposable 
        //{
        //    private readonly Action disposableAction;

        //    public Disposable(Action disposableAction)
        //    {
        //        this.disposableAction = disposableAction;
        //    }

        //    public void Dispose()
        //    {
        //        this.disposableAction();
        //    }
        //}
    }
}