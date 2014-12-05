//-------------------------------------------------------------------------------
// <copyright file="IOutgoingLogicalStepRegisterer.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;

    public interface IOutgoingLogicalStepRegisterer
    {
        IOutgoingLogicalStepRegisterer Register(Func<IOutgoingLogicalStep> stepFactory);

        IOutgoingLogicalStepRegisterer Register(IOutgoingLogicalStep step);
    }
}