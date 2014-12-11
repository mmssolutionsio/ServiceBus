//-------------------------------------------------------------------------------
// <copyright file="AlwaysRouteToDestination.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Pipeline.Outgoing;

    public class AlwaysRouteToDestination : IMessageRouter
    {
        private readonly Address destination;

        public AlwaysRouteToDestination(Address destination)
        {
            this.destination = destination;
        }

        public IReadOnlyCollection<Address> GetDestinationFor(Type messageType)
        {
            var addresses = new List<Address> { this.destination };
            return new ReadOnlyCollection<Address>(addresses);
        }
    }
}