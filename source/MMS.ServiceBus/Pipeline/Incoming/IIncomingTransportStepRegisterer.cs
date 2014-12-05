//-------------------------------------------------------------------------------
// <copyright file="IIncomingTransportStepRegisterer.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;

    public interface IIncomingTransportStepRegisterer
    {
        IIncomingTransportStepRegisterer Register(IIncomingTransportStep step);

        IIncomingTransportStepRegisterer Register(Func<IIncomingTransportStep> stepFactory);
    }
}