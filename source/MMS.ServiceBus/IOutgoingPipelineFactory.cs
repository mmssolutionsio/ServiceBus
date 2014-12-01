//-------------------------------------------------------------------------------
// <copyright file="IOutgoingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using Pipeline;

    public interface IOutgoingPipelineFactory
    {
        OutgoingPipeline Create();
    }
}