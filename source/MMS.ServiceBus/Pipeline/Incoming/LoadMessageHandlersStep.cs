//-------------------------------------------------------------------------------
// <copyright file="LoadMessageHandlersStep.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Incoming
{
    using System;
    using System.Threading.Tasks;

    public class LoadMessageHandlersStep : IIncomingLogicalStep
    {
        private readonly IHandlerRegistry registry;

        public LoadMessageHandlersStep(IHandlerRegistry registry)
        {
            this.registry = registry;
        }

        public async Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next)
        {
            var messageType = context.LogicalMessage.Instance.GetType();

            var handlers = this.registry.GetHandlers(messageType);

            foreach (var handler in handlers)
            {
                using (context.CreateSnapshot())
                {
                    var messageHandler = new MessageHandler
                        {
                            Instance = handler,
                            Invocation = (handlerInstance, message) => this.registry.InvokeHandle(handlerInstance, message, bus)
                        };

                    context.Handler = messageHandler;

                    await next()
                        .ConfigureAwait(false);

                    if (context.HandlerInvocationAbortPending)
                    {
                        break;
                    } 
                }
            }
        }
    }
}