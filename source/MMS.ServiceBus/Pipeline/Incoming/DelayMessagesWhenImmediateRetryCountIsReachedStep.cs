//-------------------------------------------------------------------------------
// <copyright file="DelayMessagesWhenImmediateRetryCountIsReachedStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;

    public class DelayMessagesWhenImmediateRetryCountIsReachedStep : IIncomingLogicalStep
    {
        public DateTime Time { get; private set; }

        public async Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next)
        {
            ExceptionDispatchInfo exceptionDispatchInfo = null;
            try
            {
                await next()
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                if (ShouldMessageBeDelayed(context))
                {
                    // We can't do async in a catch block, therefore we have to capture the exception!
                    exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                }
                else
                {
                    throw;
                }
            }

            if (exceptionDispatchInfo != null)
            {
                var message = context.TransportMessage;
                this.Time = DateTime.UtcNow;
                var scheduledEnqueueTimeUtc = this.Time
                                                  + TimeSpan.FromSeconds(DelayTimeSpanInSeconds(context));

                message.DelayedDeliveryCount++;
                await bus.Postpone(context.LogicalMessage.Instance, scheduledEnqueueTimeUtc);

                // Don't rethrow, the current message is consumed and a new message is postponed.
            }
        }

        private static double DelayTimeSpanInSeconds(IncomingLogicalContext context)
        {
            return Math.Pow(2, context.TransportMessage.DelayedDeliveryCount);
        }

        private static bool ShouldMessageBeDelayed(IncomingLogicalContext context)
        {
            return context.TransportMessage.DeliveryCount >= context.Configuration.ImmediateRetryCount 
                && context.TransportMessage.DelayedDeliveryCount < context.Configuration.DelayedRetryCount;
        }
    }
}