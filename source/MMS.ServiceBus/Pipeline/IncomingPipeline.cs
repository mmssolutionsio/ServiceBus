//-------------------------------------------------------------------------------
// <copyright file="IncomingPipeline.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class IncomingPipeline : ISupportSnapshots
    {
        private readonly Queue<IIncomingTransportPipelineStep> registeredTransportPipeline;

        private readonly Queue<IIncomingLogicalPipelineStep> registeredLogicalPipeline;

        private readonly Stack<Queue<IIncomingLogicalPipelineStep>> snapshotLogical;

        private readonly Stack<Queue<IIncomingTransportPipelineStep>> snapshotTransport;

        private Queue<IIncomingTransportPipelineStep> executingTransportPipeline;

        private Queue<IIncomingLogicalPipelineStep> executingLogicalPipeline;

        private IncomingLogicalContext currentContext;

        public IncomingPipeline()
        {
            this.snapshotTransport = new Stack<Queue<IIncomingTransportPipelineStep>>();
            this.snapshotLogical = new Stack<Queue<IIncomingLogicalPipelineStep>>();

            this.registeredLogicalPipeline = new Queue<IIncomingLogicalPipelineStep>();
            this.registeredTransportPipeline = new Queue<IIncomingTransportPipelineStep>();
        }

        public IncomingPipeline RegisterStep(IIncomingLogicalPipelineStep step)
        {
            this.registeredLogicalPipeline.Enqueue(step);

            return this;
        }

        public IncomingPipeline RegisterStep(Func<IIncomingLogicalPipelineStep> stepFactory)
        {
            this.registeredLogicalPipeline.Enqueue(new LazyLogicalStep(stepFactory));

            return this;
        }

        public IncomingPipeline RegisterStep(IIncomingTransportPipelineStep step)
        {
            this.registeredTransportPipeline.Enqueue(step);

            return this;
        }

        public IncomingPipeline RegisterStep(Func<IIncomingTransportPipelineStep> stepFactory)
        {
            this.registeredTransportPipeline.Enqueue(new LazyTransportStep(stepFactory));

            return this;
        }

        public void TakeSnapshot()
        {
            this.snapshotLogical.Push(new Queue<IIncomingLogicalPipelineStep>(this.executingLogicalPipeline));
            this.snapshotTransport.Push(new Queue<IIncomingTransportPipelineStep>(this.executingTransportPipeline));
        }

        public void DeleteSnapshot()
        {
            this.executingLogicalPipeline = this.snapshotLogical.Pop();
            this.executingTransportPipeline = this.snapshotTransport.Pop();
        }

        public virtual async Task Invoke(IBus bus, TransportMessage message)
        {
            this.executingTransportPipeline = new Queue<IIncomingTransportPipelineStep>(this.registeredTransportPipeline);
            var transportContext = new IncomingTransportContext(message);
            transportContext.SetChain(this);
            await this.InvokeTransport(transportContext, bus);

            // We assume that someone in the pipeline made logical message
            var logicalMessage = transportContext.Get<LogicalMessage>();

            this.executingLogicalPipeline = new Queue<IIncomingLogicalPipelineStep>(this.registeredLogicalPipeline);
            var logicalContext = new IncomingLogicalContext(logicalMessage, message);
            logicalContext.SetChain(this);
            this.currentContext = logicalContext;
            await this.InvokeLogical(logicalContext, bus);
        }

        public void DoNotInvokeAnyMoreHandlers()
        {
            this.currentContext.HandlerInvocationAborted = true;
        }

        private async Task InvokeLogical(IncomingLogicalContext context, IBus bus)
        {
            if (this.executingLogicalPipeline.Count == 0)
            {
                return;
            }

            IIncomingLogicalPipelineStep step = this.executingLogicalPipeline.Dequeue();

            await step.Invoke(context, bus, async () => await this.InvokeLogical(context, bus));
        }

        private async Task InvokeTransport(IncomingTransportContext context, IBus bus)
        {
            if (this.executingTransportPipeline.Count == 0)
            {
                return;
            }

            IIncomingTransportPipelineStep step = this.executingTransportPipeline.Dequeue();

            await step.Invoke(context, bus, async () => await this.InvokeTransport(context, bus));
        }

        private class LazyLogicalStep : IIncomingLogicalPipelineStep
        {
            private readonly Func<IIncomingLogicalPipelineStep> factory;

            public LazyLogicalStep(Func<IIncomingLogicalPipelineStep> factory)
            {
                this.factory = factory;
            }

            public async Task Invoke(IncomingLogicalContext context, IBus bus, Func<Task> next)
            {
                var step = this.factory();

                await step.Invoke(context, bus, next);
            }
        }

        private class LazyTransportStep : IIncomingTransportPipelineStep
        {
            private readonly Func<IIncomingTransportPipelineStep> factory;

            public LazyTransportStep(Func<IIncomingTransportPipelineStep> factory)
            {
                this.factory = factory;
            }

            public async Task Invoke(IncomingTransportContext context, IBus bus, Func<Task> next)
            {
                var step = this.factory();

                await step.Invoke(context, bus, next);
            }
        }
    }
}