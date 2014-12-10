//-------------------------------------------------------------------------------
// <copyright file="SendOnlyBus.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus
{
    using Dequeuing;
    using Pipeline.Incoming;
    using Pipeline.Outgoing;

    public class SendOnlyBus : Bus
    {
        public SendOnlyBus(SendOnlyConfiguration configuration, IOutgoingPipelineFactory outgoingPipelineFactory) : base(configuration, new NoOpDequeStrategy(), outgoingPipelineFactory, new EmptyIncomingPipelineFactory())
        {
        }
    }
}