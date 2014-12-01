//-------------------------------------------------------------------------------
// <copyright file="SendOptions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    public class SendOptions : DeliveryOptions
    {
        public SendOptions(Address address)
        {
            this.Destination = address;
        }

        public Address Destination { get; private set; }
    }
}