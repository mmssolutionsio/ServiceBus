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

        protected Address(string address)
        {
            this.address = address;
        }

        // Currently hard coded to queues
        public static Address Parse(string address)
        {
            return new Queue(address);
        }

        public static bool operator ==(Address left, Address right)
        {
            return object.Equals(left, right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !object.Equals(left, right);
        }

        public static implicit operator string(Address @from)
        {
            return @from.address;
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
    }
}