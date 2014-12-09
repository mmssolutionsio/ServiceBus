//-------------------------------------------------------------------------------
// <copyright file="IIncomingTransportStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface IIncomingTransportStep
    {
        Task Invoke([NotNull] IncomingTransportContext context, [NotNull] IBusForHandler bus, [NotNull] Func<Task> next);
    }
}