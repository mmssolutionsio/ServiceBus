namespace MMS.Common.ServiceBusWrapper
{
    using System;
    using System.Collections.Generic;

    public class Address : IEquatable<Address>
    {
        public bool Equals(Address other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(address, other.address);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Address)obj);
        }

        public override int GetHashCode()
        {
            return address.GetHashCode();
        }

        public static bool operator ==(Address left, Address right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !Equals(left, right);
        }

        private readonly string address;

        protected Address(string address)
        {
            this.address = address;
        }

        public static implicit operator string(Address @from)
        {
            return @from.address;
        }

        // Currently hard coded to queues
        public static Address Parse(string address)
        {
            return new Queue(address);
        }
    }

    public abstract class DeliveryOptions
    {
        protected DeliveryOptions()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Headers { get; private set; }

        public Address ReplyToAddress { get; set; }
    }
}