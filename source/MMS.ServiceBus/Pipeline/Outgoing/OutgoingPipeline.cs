//-------------------------------------------------------------------------------
// <copyright file="OutgoingPipeline.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class OutgoingPipeline : IOutgoingTransportStepRegisterer, IOutgoingLogicalStepRegisterer, ISupportSnapshots
    {
        private readonly Queue<IOutgoingTransportStep> registeredTransportPipelineSteps;

        private readonly Queue<IOutgoingLogicalStep> registeredlogicalPipelineSteps;

        private readonly Stack<Queue<IOutgoingLogicalStep>> snapshotLogical;

        private readonly Stack<Queue<IOutgoingTransportStep>> snapshotTransport;

        private Queue<IOutgoingTransportStep> executingTransportPipeline;

        private Queue<IOutgoingLogicalStep> executingLogicalPipeline;

        public OutgoingPipeline()
        {
            this.snapshotTransport = new Stack<Queue<IOutgoingTransportStep>>();
            this.snapshotLogical = new Stack<Queue<IOutgoingLogicalStep>>();

            this.registeredlogicalPipelineSteps = new Queue<IOutgoingLogicalStep>();
            this.registeredTransportPipelineSteps = new Queue<IOutgoingTransportStep>();
        }

        public IOutgoingTransportStepRegisterer Transport
        {
            get { return this; }
        }

        public IOutgoingLogicalStepRegisterer Logical
        {
            get { return this; }
        }

        IOutgoingLogicalStepRegisterer IOutgoingLogicalStepRegisterer.Register(IOutgoingLogicalStep step)
        {
            this.registeredlogicalPipelineSteps.Enqueue(step);

            return this;
        }

        IOutgoingLogicalStepRegisterer IOutgoingLogicalStepRegisterer.Register(Func<IOutgoingLogicalStep> stepFactory)
        {
            this.registeredlogicalPipelineSteps.Enqueue(new LazyLogicalStep(stepFactory));

            return this;
        }

        IOutgoingTransportStepRegisterer IOutgoingTransportStepRegisterer.Register(IOutgoingTransportStep step)
        {
            this.registeredTransportPipelineSteps.Enqueue(step);

            return this;
        }

        IOutgoingTransportStepRegisterer IOutgoingTransportStepRegisterer.Register(Func<IOutgoingTransportStep> stepFactory)
        {
            this.registeredTransportPipelineSteps.Enqueue(new LazyTransportStep(stepFactory));

            return this;
        }

        public void TakeSnapshot()
        {
            this.snapshotLogical.Push(new Queue<IOutgoingLogicalStep>(this.executingLogicalPipeline));
            this.snapshotTransport.Push(new Queue<IOutgoingTransportStep>(this.executingTransportPipeline));
        }

        public void DeleteSnapshot()
        {
            this.executingLogicalPipeline = this.snapshotLogical.Pop();
            this.executingTransportPipeline = this.snapshotTransport.Pop();
        }

        public virtual async Task Invoke(LogicalMessage outgoingLogicalMessage, DeliveryOptions options, EndpointConfiguration.ReadOnly configuration, TransportMessage incomingTransportMessage = null)
        {
            this.executingLogicalPipeline = new Queue<IOutgoingLogicalStep>(this.registeredlogicalPipelineSteps);
            var logicalContext = new OutgoingLogicalContext(outgoingLogicalMessage, options, configuration);
            logicalContext.SetChain(this);
            await this.InvokeLogical(logicalContext)
                .ConfigureAwait(false);

            // We assume that someone in the pipeline made transport message
            var outgoingTransportMessage = logicalContext.Get<TransportMessage>();

            this.executingTransportPipeline = new Queue<IOutgoingTransportStep>(this.registeredTransportPipelineSteps);
            var transportContext = new OutgoingTransportContext(outgoingLogicalMessage, outgoingTransportMessage, options, configuration, incomingTransportMessage);
            transportContext.SetChain(this);
            await this.InvokeTransport(transportContext)
                .ConfigureAwait(false);
        }

        private Task InvokeLogical(OutgoingLogicalContext context)
        {
            if (this.executingLogicalPipeline.Count == 0)
            {
                return Task.FromResult(0);
            }

            IOutgoingLogicalStep step = this.executingLogicalPipeline.Dequeue();

            return step.Invoke(context, () => this.InvokeLogical(context));
        }

        private Task InvokeTransport(OutgoingTransportContext context)
        {
            if (this.executingTransportPipeline.Count == 0)
            {
                return Task.FromResult(0);
            }

            IOutgoingTransportStep step = this.executingTransportPipeline.Dequeue();

            return step.Invoke(context, () => this.InvokeTransport(context));
        }

        private class LazyLogicalStep : IOutgoingLogicalStep
        {
            private readonly Func<IOutgoingLogicalStep> factory;

            public LazyLogicalStep(Func<IOutgoingLogicalStep> factory)
            {
                this.factory = factory;
            }

            public Task Invoke(OutgoingLogicalContext context, Func<Task> next)
            {
                var step = this.factory();

                return step.Invoke(context, next);
            }
        }

        private class LazyTransportStep : IOutgoingTransportStep
        {
            private readonly Func<IOutgoingTransportStep> factory;

            public LazyTransportStep(Func<IOutgoingTransportStep> factory)
            {
                this.factory = factory;
            }

            public Task Invoke(OutgoingTransportContext context, Func<Task> next)
            {
                var step = this.factory();

                return step.Invoke(context, next);
            }
        }
    }
}