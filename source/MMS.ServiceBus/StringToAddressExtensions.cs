//-------------------------------------------------------------------------------
// <copyright file="StringToAddressExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    public static class StringToAddressExtensions
    {
        public static Address Parse(this string address)
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
    }
}