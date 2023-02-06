//-------------------------------------------------------------------------------
// <copyright file="IOutgoingLogicalStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Threading.Tasks;

    public interface IOutgoingLogicalStep
    {
        Task Invoke(OutgoingLogicalContext context, Func<Task> next);
    }
}