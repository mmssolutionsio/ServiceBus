//-------------------------------------------------------------------------------
// <copyright file="MessageRouterExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public static class MessageRouterExtensions
    {
        public static IReadOnlyCollection<Address> NoDestination(this IMessageRouter router)
        {
            return new ReadOnlyCollection<Address>(new List<Address>());
        }

        public static IReadOnlyCollection<Address> To(this IMessageRouter router, Queue queue)
        {
            return router.To(queue);
        }

        public static IReadOnlyCollection<Address> To(this IMessageRouter router, Topic topic)
        {
            return new ReadOnlyCollection<Address>(new List<Address> { topic });
        } 
    }
}