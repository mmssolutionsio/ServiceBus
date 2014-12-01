//-------------------------------------------------------------------------------
// <copyright file="IIncomingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using Pipeline;
    using Pipeline.Incoming;

    public interface IIncomingPipelineFactory
    {
        IncomingPipeline Create();
    }
}