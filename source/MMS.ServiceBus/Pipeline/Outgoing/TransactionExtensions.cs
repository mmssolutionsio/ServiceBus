//-------------------------------------------------------------------------------
// <copyright file="TransactionExtensions.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Threading.Tasks;
    using System.Transactions;

    internal static class TransactionExtensions
    {
        public static Task ExecuteAsyncWithEnlistmentIfNecessary(this Transaction transaction, Func<Task> func)
        {
            if (transaction != null)
            {
                return Transaction.Current.EnlistVolatileAsync(new SendResourceManager(func), EnlistmentOptions.None);
            }

            return func();
        }

        private static Task EnlistVolatileAsync(
            this Transaction transaction,
            IEnlistmentNotification enlistmentNotification,
            EnlistmentOptions enlistmentOptions)
        {
            return Task.FromResult(transaction.EnlistVolatile(enlistmentNotification, enlistmentOptions));
        }
    }
}