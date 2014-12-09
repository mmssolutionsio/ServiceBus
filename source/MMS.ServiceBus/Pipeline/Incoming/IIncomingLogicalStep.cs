//-------------------------------------------------------------------------------
// <copyright file="IIncomingLogicalStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Threading.Tasks;

    public interface IIncomingLogicalStep
    {
        Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next);
    }
}