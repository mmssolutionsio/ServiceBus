//-------------------------------------------------------------------------------
// <copyright file="Queue.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    public class Queue : Address
    {
        public Queue(string address)
            : base(address)
        {
            // TODO Validate here
        }
    }
}