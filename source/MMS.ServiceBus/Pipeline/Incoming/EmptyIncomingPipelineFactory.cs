//-------------------------------------------------------------------------------
// <copyright file="EmptyIncomingPipelineFactory.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System.Threading.Tasks;

    internal class EmptyIncomingPipelineFactory : IIncomingPipelineFactory
    {
        public Task WarmupAsync()
        {
            return Task.FromResult(0);
        }

        public IncomingPipeline Create()
        {
            return new IncomingPipeline();
        }

        public Task CooldownAsync()
        {
            return Task.FromResult(0);
        }
    }
}