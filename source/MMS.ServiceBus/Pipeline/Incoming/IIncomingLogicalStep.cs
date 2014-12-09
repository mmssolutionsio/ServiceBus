//-------------------------------------------------------------------------------
// <copyright file="IIncomingLogicalStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface IIncomingLogicalStep
    {
        Task Invoke([NotNull] IncomingLogicalContext context, [NotNull] IBusForHandler bus, [NotNull] Func<Task> next);
    }
}