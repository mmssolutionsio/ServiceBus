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

    public class OutgoingPipeline : ISupportSnapshots
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

        public OutgoingPipeline RegisterStep(IOutgoingLogicalStep step)
        {
            this.registeredlogicalPipelineSteps.Enqueue(step);

            return this;
        }

        public OutgoingPipeline RegisterStep(Func<IOutgoingLogicalStep> stepFactory)
        {
            this.registeredlogicalPipelineSteps.Enqueue(new LazyLogicalStep(stepFactory));

            return this;
        }

        public OutgoingPipeline RegisterStep(IOutgoingTransportStep step)
        {
            this.registeredTransportPipelineSteps.Enqueue(step);

            return this;
        }

        public OutgoingPipeline RegisterStep(Func<IOutgoingTransportStep> stepFactory)
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

        public virtual async Task Invoke(LogicalMessage message, DeliveryOptions options)
        {
            this.executingLogicalPipeline = new Queue<IOutgoingLogicalStep>(this.registeredlogicalPipelineSteps);
            var logicalContext = new OutgoingLogicalContext(message, options);
            logicalContext.SetChain(this);
            await this.InvokeLogical(logicalContext);

            // We assume that someone in the pipeline made transport message
            var transportMessage = logicalContext.Get<TransportMessage>();

            this.executingTransportPipeline = new Queue<IOutgoingTransportStep>(this.registeredTransportPipelineSteps);
            var transportContext = new OutgoingTransportContext(message, transportMessage, options);
            transportContext.SetChain(this);
            await this.InvokeTransport(transportContext);
        }

        private async Task InvokeLogical(OutgoingLogicalContext context)
        {
            if (this.executingLogicalPipeline.Count == 0)
            {
                return;
            }

            IOutgoingLogicalStep step = this.executingLogicalPipeline.Dequeue();

            await step.Invoke(context, async () => await this.InvokeLogical(context));
        }

        private async Task InvokeTransport(OutgoingTransportContext context)
        {
            if (this.executingTransportPipeline.Count == 0)
            {
                return;
            }

            IOutgoingTransportStep step = this.executingTransportPipeline.Dequeue();

            await step.Invoke(context, async () => await this.InvokeTransport(context));
        }

        private class LazyLogicalStep : IOutgoingLogicalStep
        {
            private readonly Func<IOutgoingLogicalStep> factory;

            public LazyLogicalStep(Func<IOutgoingLogicalStep> factory)
            {
                this.factory = factory;
            }

            public async Task Invoke(OutgoingLogicalContext context, Func<Task> next)
            {
                var step = this.factory();

                await step.Invoke(context, next);
            }
        }

        private class LazyTransportStep : IOutgoingTransportStep
        {
            private readonly Func<IOutgoingTransportStep> factory;

            public LazyTransportStep(Func<IOutgoingTransportStep> factory)
            {
                this.factory = factory;
            }

            public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
            {
                var step = this.factory();

                await step.Invoke(context, next);
            }
        }
    }
}