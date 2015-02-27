//-------------------------------------------------------------------------------
// <copyright file="MessageUnit.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;
    using System.Transactions;

    public static class DisposableExtensions
    {
        public static async Task UsingAsync(this IDisposable disposable, Func<Task> scoped)
        {
            ExceptionDispatchInfo exceptionDispatchInfo = null;
            try
            {
                await scoped().ConfigureAwait(false);

                if (Transaction.Current != null)
                {
                    Transaction.Current.EnlistVolatile(new DisposeEnlistment(disposable), EnlistmentOptions.None);
                    return;
                }
            }
            catch (Exception e)
            {
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
            }

            disposable.Dispose();

            if (exceptionDispatchInfo != null)
            {
                exceptionDispatchInfo.Throw();
            }
        }

        private class DisposeEnlistment : IEnlistmentNotification
        {
            private readonly IDisposable disposable;

            public DisposeEnlistment(IDisposable disposable)
            {
                this.disposable = disposable;
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.Prepared();
            }

            public void Commit(Enlistment enlistment)
            {
                this.disposable.Dispose();
                enlistment.Done();
            }

            public void Rollback(Enlistment enlistment)
            {
                this.disposable.Dispose();
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }
        }
    }
}