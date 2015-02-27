//-------------------------------------------------------------------------------
// <copyright file="DispatchToTransportStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Threading.Tasks;

    public class DispatchToTransportStep : IOutgoingTransportStep
    {
        private readonly ISendMessages sender;
        private readonly IPublishMessages publisher;

        public DispatchToTransportStep(ISendMessages sender, IPublishMessages publisher)
        {
            this.publisher = publisher;
            this.sender = sender;
        }

        public Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            return context.Enlistment.Enlist(new FunkyTown(() => this.InvokeInternal(context, next)));
        }

        private async Task InvokeInternal(OutgoingTransportContext context, Func<Task> next)
        {
            var sendOptions = context.Options as SendOptions;
            if (sendOptions != null)
            {
                await this.sender.SendAsync(context.OutgoingTransportMessage, sendOptions)
                    .ConfigureAwait(false);
            }

            var publishOptions = context.Options as PublishOptions;
            if (publishOptions != null)
            {
                await this.publisher.PublishAsync(context.OutgoingTransportMessage, publishOptions)
                    .ConfigureAwait(false);
            }

            await next()
                .ConfigureAwait(false);
        }

        private class FunkyTown : ITransaction
        {
            private readonly Func<Task> func1;

            public FunkyTown(Func<Task> func)
            {
                this.func1 = func;
            }

            public Task CompleteAsync()
            {
                return this.func1();
            }

            public Task RollbackAsync()
            {
                return Task.FromResult(0);
            }
        }
    }
}