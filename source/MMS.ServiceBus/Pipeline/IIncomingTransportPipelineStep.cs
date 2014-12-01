//-------------------------------------------------------------------------------
// <copyright file="IIncomingTransportPipelineStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Threading.Tasks;

    public interface IIncomingTransportPipelineStep
    {
        Task Invoke(IncomingTransportContext context, IBus bus, Func<Task> next);
    }
}