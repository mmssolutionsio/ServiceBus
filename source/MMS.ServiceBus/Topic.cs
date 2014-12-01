//-------------------------------------------------------------------------------
// <copyright file="Topic.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    public class Topic : Address
    {
        public Topic(string address)
            : base(address)
        {
            // TODO Validate here
        }
    }
}