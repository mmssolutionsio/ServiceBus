//-------------------------------------------------------------------------------
// <copyright file="IOutgoingTransportStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Threading.Tasks;

    public interface IOutgoingTransportStep
    {
        Task Invoke(OutgoingTransportContext context, Func<Task> next);
    }
}