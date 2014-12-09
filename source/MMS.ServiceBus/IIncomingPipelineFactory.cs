//-------------------------------------------------------------------------------
// <copyright file="IIncomingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System.Threading.Tasks;
    using Pipeline.Incoming;

    public interface IIncomingPipelineFactory
    {
        Task WarmupAsync();

        IncomingPipeline Create();

        Task CooldownAsync();
    }
}