//-------------------------------------------------------------------------------
// <copyright file="IIncomingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System.Threading.Tasks;

    public interface IIncomingPipelineFactory
    {
        Task WarmupAsync();

        IncomingPipeline Create();

        Task CooldownAsync();
    }
}