//-------------------------------------------------------------------------------
// <copyright file="IMessageRouter.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Collections.Generic;

    public interface IMessageRouter
    {
        IReadOnlyCollection<Address> GetDestinationFor(Type messageType);
    }
}