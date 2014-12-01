//-------------------------------------------------------------------------------
// <copyright file="MessageRouter.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
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