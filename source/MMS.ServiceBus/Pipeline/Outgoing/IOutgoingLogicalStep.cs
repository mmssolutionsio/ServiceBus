//-------------------------------------------------------------------------------
// <copyright file="IOutgoingLogicalStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface IOutgoingLogicalStep
    {
        Task Invoke([NotNull] OutgoingLogicalContext context, [NotNull] Func<Task> next);
    }
}