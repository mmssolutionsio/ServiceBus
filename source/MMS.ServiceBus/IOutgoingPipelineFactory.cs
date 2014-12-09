//-------------------------------------------------------------------------------
// <copyright file="IOutgoingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using System.Threading.Tasks;
    using Pipeline.Outgoing;

    public interface IOutgoingPipelineFactory
    {
        Task WarmupAsync();

        OutgoingPipeline Create();

        Task CooldownAsync();
    }
}