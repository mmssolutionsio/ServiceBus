//-------------------------------------------------------------------------------
// <copyright file="IOutgoingLogicalPipelineStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Threading.Tasks;

    public interface IOutgoingLogicalPipelineStep
    {
        Task Invoke(OutgoingLogicalContext context, Func<Task> next);
    }
}