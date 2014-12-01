//-------------------------------------------------------------------------------
// <copyright file="IncomingPipeline.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class IncomingPipeline : ISupportSnapshots
    {
        private readonly Queue<IIncomingTransportStep> registeredTransportPipeline;

        private readonly Queue<IIncomingLogicalStep> registeredLogicalPipeline;

        private readonly Stack<Queue<IIncomingLogicalStep>> snapshotLogical;

        private readonly Stack<Queue<IIncomingTransportStep>> snapshotTransport;

        private Queue<IIncomingTransportStep> executingTransportPipeline;

        private Queue<IIncomingLogicalStep> executingLogicalPipeline;

        private IncomingLogicalContext currentContext;

        public IncomingPipeline()
        {
            this.snapshotTransport = new Stack<Queue<IIncomingTransportStep>>();
            this.snapshotLogical = new Stack<Queue<IIncomingLogicalStep>>();

            this.registeredLogicalPipeline = new Queue<IIncomingLogicalStep>();
            this.registeredTransportPipeline = new Queue<IIncomingTransportStep>();
        }

        public IncomingPipeline RegisterStep(IIncomingLogicalStep step)
        {
            this.registeredLogicalPipeline.Enqueue(step);

            return this;
        }

        public IncomingPipeline RegisterStep(Func<IIncomingLogicalStep> stepFactory)
        {
            this.registeredLogicalPipeline.Enqueue(new LazyLogicalStep(stepFactory));

            return this;
        }

        public IncomingPipeline RegisterStep(IIncomingTransportStep step)
        {
            this.registeredTransportPipeline.Enqueue(step);

            return this;
        }

        public IncomingPipeline RegisterStep(Func<IIncomingTransportStep> stepFactory)
        {
            this.registeredTransportPipeline.Enqueue(new LazyTransportStep(stepFactory));

            return this;
        }

        public void TakeSnapshot()
        {
            this.snapshotLogical.Push(new Queue<IIncomingLogicalStep>(this.executingLogicalPipeline));
            this.snapshotTransport.Push(new Queue<IIncomingTransportStep>(this.executingTransportPipeline));
        }

        public void DeleteSnapshot()
        {
            this.executingLogicalPipeline = this.snapshotLogical.Pop();
            this.executingTransportPipeline = this.snapshotTransport.Pop();
        }

        public async Task Invoke(IBus bus, TransportMessage message)
        {
            this.executingTransportPipeline = new Queue<IIncomingTransportStep>(this.registeredTransportPipeline);
            var transportContext = new IncomingTransportContext(message);
            transportContext.SetChain(this);
            await this.InvokeTransport(transportContext, bus);

            // We assume that someone in the pipeline made logical message
            var logicalMessage = transportContext.Get<LogicalMessage>();

            this.executingLogicalPipeline = new Queue<IIncomingLogicalStep>(this.registeredLogicalPipeline);
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

            IIncomingLogicalStep step = this.executingLogicalPipeline.Dequeue();

            await step.Invoke(context, bus, async () => await this.InvokeLogical(context, bus));
        }

        private async Task InvokeTransport(IncomingTransportContext context, IBus bus)
        {
            if (this.executingTransportPipeline.Count == 0)
            {
                return;
            }

            IIncomingTransportStep step = this.executingTransportPipeline.Dequeue();

            await step.Invoke(context, bus, async () => await this.InvokeTransport(context, bus));
        }

        private class LazyLogicalStep : IIncomingLogicalStep
        {
            private readonly Func<IIncomingLogicalStep> factory;

            public LazyLogicalStep(Func<IIncomingLogicalStep> factory)
            {
                this.factory = factory;
            }

            public async Task Invoke(IncomingLogicalContext context, IBus bus, Func<Task> next)
            {
                var step = this.factory();

                await step.Invoke(context, bus, next);
            }
        }

        private class LazyTransportStep : IIncomingTransportStep
        {
            private readonly Func<IIncomingTransportStep> factory;

            public LazyTransportStep(Func<IIncomingTransportStep> factory)
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