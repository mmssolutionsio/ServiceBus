namespace MMS.ServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class AlwaysRouteToDestination : MessageRouter
    {
        private readonly Address destination;

        public AlwaysRouteToDestination(Address destination)
        {
            this.destination = destination;
        }

        public override IReadOnlyCollection<Address> GetDestinationFor(Type messageType)
        {
            var addresses = new List<Address> { this.destination };
            return new ReadOnlyCollection<Address>(addresses);
        }
    }
}