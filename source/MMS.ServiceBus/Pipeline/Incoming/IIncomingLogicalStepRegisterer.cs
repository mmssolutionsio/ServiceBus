//-------------------------------------------------------------------------------
// <copyright file="IIncomingLogicalStepRegisterer.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;

    public interface IIncomingLogicalStepRegisterer
    {
        IIncomingLogicalStepRegisterer Register(IIncomingLogicalStep step);

        IIncomingLogicalStepRegisterer Register(Func<IIncomingLogicalStep> stepFactory);
    }
}