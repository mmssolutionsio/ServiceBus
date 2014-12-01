//-------------------------------------------------------------------------------
// <copyright file="IncomingTransportContext.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    public class IncomingTransportContext : Context
    {
        public IncomingTransportContext(TransportMessage message)
        {
            this.Set(message);
        }

        public TransportMessage TransportMessage
        {
            get
            {
                return this.Get<TransportMessage>();
            }
        }
    }
}