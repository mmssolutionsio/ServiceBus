//-------------------------------------------------------------------------------
// <copyright file="IOutgoingTransportStepRegisterer.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;

    public interface IOutgoingTransportStepRegisterer
    {
        IOutgoingTransportStepRegisterer Register(Func<IOutgoingTransportStep> stepFactory);

        IOutgoingTransportStepRegisterer Register(IOutgoingTransportStep step);
    }
}