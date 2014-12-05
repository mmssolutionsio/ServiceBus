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

    public class IncomingPipeline : IIncomingTransportStepRegisterer, IIncomingLogicalStepRegisterer, ISupportSnapshots
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

        public IIncomingTransportStepRegisterer Transport
        {
            get { return this; }
        }

        public IIncomingLogicalStepRegisterer Logical
        {
            get { return this; }
        }

        IIncomingLogicalStepRegisterer IIncomingLogicalStepRegisterer.Register(IIncomingLogicalStep step)
        {
            this.registeredLogicalPipeline.Enqueue(step);

            return this;
        }

        IIncomingLogicalStepRegisterer IIncomingLogicalStepRegisterer.Register(Func<IIncomingLogicalStep> stepFactory)
        {
            this.registeredLogicalPipeline.Enqueue(new LazyLogicalStep(stepFactory));

            return this;
        }

        IIncomingTransportStepRegisterer IIncomingTransportStepRegisterer.Register(IIncomingTransportStep step)
        {
            this.registeredTransportPipeline.Enqueue(step);

            return this;
        }

        IIncomingTransportStepRegisterer IIncomingTransportStepRegisterer.Register(Func<IIncomingTransportStep> stepFactory)
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

        public async Task Invoke(IBus bus, TransportMessage message, EndpointConfiguration.ReadOnly configuration)
        {
            this.executingTransportPipeline = new Queue<IIncomingTransportStep>(this.registeredTransportPipeline);
            var transportContext = new IncomingTransportContext(message, configuration);
            transportContext.SetChain(this);
            await this.InvokeTransport(transportContext, bus)
                .ConfigureAwait(false);

            // We assume that someone in the pipeline made logical message
            var logicalMessage = transportContext.Get<LogicalMessage>();

            this.executingLogicalPipeline = new Queue<IIncomingLogicalStep>(this.registeredLogicalPipeline);
            var logicalContext = new IncomingLogicalContext(logicalMessage, message, configuration);
            logicalContext.SetChain(this);
            this.currentContext = logicalContext;
            await this.InvokeLogical(logicalContext, bus)
                .ConfigureAwait(false);
        }

        public void DoNotInvokeAnyMoreHandlers()
        {
            this.currentContext.AbortHandlerInvocation();
        }

        private Task InvokeLogical(IncomingLogicalContext context, IBus bus)
        {
            if (this.executingLogicalPipeline.Count == 0)
            {
                return Task.FromResult(0);
            }

            IIncomingLogicalStep step = this.executingLogicalPipeline.Dequeue();

            return step.Invoke(context, bus, () => this.InvokeLogical(context, bus));
        }

        private Task InvokeTransport(IncomingTransportContext context, IBus bus)
        {
            if (this.executingTransportPipeline.Count == 0)
            {
                return Task.FromResult(0);
            }

            IIncomingTransportStep step = this.executingTransportPipeline.Dequeue();

            return step.Invoke(context, bus, () => this.InvokeTransport(context, bus));
        }

        private class LazyLogicalStep : IIncomingLogicalStep
        {
            private readonly Func<IIncomingLogicalStep> factory;

            public LazyLogicalStep(Func<IIncomingLogicalStep> factory)
            {
                this.factory = factory;
            }

            public Task Invoke(IncomingLogicalContext context, IBus bus, Func<Task> next)
            {
                var step = this.factory();

                return step.Invoke(context, bus, next);
            }
        }

        private class LazyTransportStep : IIncomingTransportStep
        {
            private readonly Func<IIncomingTransportStep> factory;

            public LazyTransportStep(Func<IIncomingTransportStep> factory)
            {
                this.factory = factory;
            }

            public Task Invoke(IncomingTransportContext context, IBus bus, Func<Task> next)
            {
                var step = this.factory();

                return step.Invoke(context, bus, next);
            }
        }
    }
}