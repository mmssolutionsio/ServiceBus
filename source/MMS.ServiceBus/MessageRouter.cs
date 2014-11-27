namespace MMS.Common.ServiceBusWrapper
{
    using System;
    using System.Collections.Generic;

    public class MessageRouter
    {
        public virtual IReadOnlyCollection<Address> GetDestinationFor(Type messageType)
        {
            return new[] { new Queue("NachrichtenEmpfangen") };
        }
    }
}