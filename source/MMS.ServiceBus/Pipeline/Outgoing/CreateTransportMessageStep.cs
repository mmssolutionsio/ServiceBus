//-------------------------------------------------------------------------------
// <copyright file="CreateTransportMessageStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Threading.Tasks;

    public class CreateTransportMessageStep : IOutgoingLogicalStep
    {
        public async Task Invoke(OutgoingLogicalContext context, Func<Task> next)
        {
            var message = new TransportMessage();

            context.Set(message);

            await next();
        }
    }
}