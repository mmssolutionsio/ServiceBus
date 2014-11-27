namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class OutgoingPipeline : ISupportSnapshots
    {
        private readonly Queue<IOutgoingTransportPipelineStep> registeredTransportPipelineSteps;

        private readonly Queue<IOutgoingLogicalPipelineStep> registeredlogicalPipelineSteps;

        private readonly Stack<Queue<IOutgoingLogicalPipelineStep>> snapshotLogical;

        private readonly Stack<Queue<IOutgoingTransportPipelineStep>> snapshotTransport;

        private Queue<IOutgoingTransportPipelineStep> executingTransportPipeline;

        private Queue<IOutgoingLogicalPipelineStep> executingLogicalPipeline;

        public OutgoingPipeline()
        {
            this.snapshotTransport = new Stack<Queue<IOutgoingTransportPipelineStep>>();
            this.snapshotLogical = new Stack<Queue<IOutgoingLogicalPipelineStep>>();

            this.registeredlogicalPipelineSteps = new Queue<IOutgoingLogicalPipelineStep>();
            this.registeredTransportPipelineSteps = new Queue<IOutgoingTransportPipelineStep>();
        }

        public OutgoingPipeline RegisterStep(IOutgoingLogicalPipelineStep step)
        {
            this.registeredlogicalPipelineSteps.Enqueue(step);

            return this;
        }

        public OutgoingPipeline RegisterStep(Func<IOutgoingLogicalPipelineStep> stepFactory)
        {
            this.registeredlogicalPipelineSteps.Enqueue(new LazyLogicalStep(stepFactory));

            return this;
        }

        public OutgoingPipeline RegisterStep(IOutgoingTransportPipelineStep step)
        {
            this.registeredTransportPipelineSteps.Enqueue(step);

            return this;
        }

        public OutgoingPipeline RegisterStep(Func<IOutgoingTransportPipelineStep> stepFactory)
        {
            this.registeredTransportPipelineSteps.Enqueue(new LazyTransportStep(stepFactory));

            return this;
        }

        public void TakeSnapshot()
        {
            this.snapshotLogical.Push(new Queue<IOutgoingLogicalPipelineStep>(this.executingLogicalPipeline));
            this.snapshotTransport.Push(new Queue<IOutgoingTransportPipelineStep>(this.executingTransportPipeline));
        }

        public void DeleteSnapshot()
        {
            this.executingLogicalPipeline = this.snapshotLogical.Pop();
            this.executingTransportPipeline = this.snapshotTransport.Pop();
        }

        public virtual async Task Invoke(LogicalMessage message, DeliveryOptions options)
        {
            this.executingLogicalPipeline = new Queue<IOutgoingLogicalPipelineStep>(this.registeredlogicalPipelineSteps);
            var logicalContext = new OutgoingLogicalContext(message, options);
            logicalContext.SetChain(this);
            await this.InvokeLogical(logicalContext);

            // We assume that someone in the pipeline made transport message
            var transportMessage = logicalContext.Get<TransportMessage>();

            this.executingTransportPipeline = new Queue<IOutgoingTransportPipelineStep>(this.registeredTransportPipelineSteps);
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

            IOutgoingLogicalPipelineStep step = this.executingLogicalPipeline.Dequeue();

            await step.Invoke(context, async () => await this.InvokeLogical(context));
        }

        private async Task InvokeTransport(OutgoingTransportContext context)
        {
            if (this.executingTransportPipeline.Count == 0)
            {
                return;
            }

            IOutgoingTransportPipelineStep step = this.executingTransportPipeline.Dequeue();

            await step.Invoke(context, async () => await this.InvokeTransport(context));
        }

        private class LazyLogicalStep : IOutgoingLogicalPipelineStep
        {
            private readonly Func<IOutgoingLogicalPipelineStep> factory;

            public LazyLogicalStep(Func<IOutgoingLogicalPipelineStep> factory)
            {
                this.factory = factory;
            }

            public async Task Invoke(OutgoingLogicalContext context, Func<Task> next)
            {
                var step = this.factory();

                await step.Invoke(context, next);
            }
        }

        private class LazyTransportStep : IOutgoingTransportPipelineStep
        {
            private readonly Func<IOutgoingTransportPipelineStep> factory;

            public LazyTransportStep(Func<IOutgoingTransportPipelineStep> factory)
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