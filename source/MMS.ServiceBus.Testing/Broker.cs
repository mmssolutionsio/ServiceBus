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
            if (!this.units.ContainsKey(unit.Endpoint))
            {
                this.units.Add(unit.Endpoint, new List<MessageUnit>());
            }

            unit.SetOutgoing(this.Outgoing);
            this.units[unit.Endpoint].Add(unit);

            return this;
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

        private async Task Outgoing(TransportMessage message)
        {
            var address = Address.Parse(message.Headers[AcceptanceTestHeaders.Destination]);

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