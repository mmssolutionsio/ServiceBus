//-------------------------------------------------------------------------------
// <copyright file="Address.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System;

    public class Address : IEquatable<Address>
    {
        private readonly string address;
        private readonly string schema;

        protected Address(string address, string schema)
        {
            this.schema = schema;
            this.address = address;
        }

        public string Destination
        {
            get { return this.address.Replace(this.schema, string.Empty); }
        }

        public static Address Parse(string address)
        {
            Queue queue;
            if (Queue.TryParse(address, out queue))
            {
                return queue;
            }

            Topic topic;
            if (Topic.TryParse(address, out topic))
            {
                return topic;
            }

            return null;
        }

        public static bool operator ==(Address left, Address right)
        {
            return object.Equals(left, right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !object.Equals(left, right);
        }

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

            return string.Equals(this.address, other.address);
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

            return this.Equals((Address)obj);
        }

        public override int GetHashCode()
        {
            return this.address.GetHashCode();
        }

        public override string ToString()
        {
            return this.address;
        }
    }
}