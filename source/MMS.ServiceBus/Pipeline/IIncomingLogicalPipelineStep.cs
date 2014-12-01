//-------------------------------------------------------------------------------
// <copyright file="IIncomingLogicalPipelineStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Threading.Tasks;

    public interface IIncomingLogicalPipelineStep
    {
        Task Invoke(IncomingLogicalContext context, IBus bus, Func<Task> next);
    }
}