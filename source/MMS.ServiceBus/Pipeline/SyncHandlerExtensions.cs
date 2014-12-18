//-------------------------------------------------------------------------------
// <copyright file="SyncHandlerExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline
{
    public static class SyncHandlerExtensions
    {
        public static IHandleMessageAsync<TMessage> AsAsync<TMessage>(this IHandleMessage<TMessage> handler)
        {
            return new SyncAsAsyncHandlerDecorator<TMessage>(handler);
        }
    }
}