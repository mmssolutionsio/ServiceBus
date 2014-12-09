//-------------------------------------------------------------------------------
// <copyright file="IOutgoingTransportStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface IOutgoingTransportStep
    {
        Task Invoke([NotNull] OutgoingTransportContext context, [NotNull] Func<Task> next);
    }
}