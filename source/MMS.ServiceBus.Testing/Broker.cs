//-------------------------------------------------------------------------------
// <copyright file="Broker.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Testing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Broker
    {
        private readonly Dictionary<Address, IList<MessageUnit>> units; 

        public Broker()
        {
            this.units = new Dictionary<Address, IList<MessageUnit>>();
        }

        public Broker Register(MessageUnit unit)
        {
            return this.Register(unit, unit.Endpoint);
        }

        public Broker Register(MessageUnit unit, Topic topic)
        {
            return this.Register(unit, (Address)topic);
        }

        public async Task StartAsync()
        {
            foreach (MessageUnit unit in this.units.SelectMany(x => x.Value))
            {
                await unit.StartAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task StopAsync()
        {
            foreach (MessageUnit unit in this.units.SelectMany(x => x.Value).Reverse())
            {
                await unit.StopAsync()
                    .ConfigureAwait(false);
            }
        }

        private Broker Register(MessageUnit unit, Address address)
        {
            if (!this.units.ContainsKey(address))
            {
                this.units.Add(address, new List<MessageUnit>());

                unit.SetOutgoing(this.Outgoing);
                this.units[address].Add(unit);
            }

            return this;
        }

        private async Task Outgoing(TransportMessage message)
        {
            var address = message.Headers[AcceptanceTestHeaders.Destination].Parse();

            IList<MessageUnit> destinations;
            if (!this.units.TryGetValue(address, out destinations))
            {
                destinations = new MessageUnit[] { };
            }

            foreach (MessageUnit unit in destinations)
            {
                await unit.HandOver(message)
                    .ConfigureAwait(false);
            }
        }
    }
}