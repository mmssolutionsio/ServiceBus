//-------------------------------------------------------------------------------
// <copyright file="SendRessourceManager.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Pipeline.Outgoing
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Transactions;

    internal class SendRessourceManager : IEnlistmentNotification
    {
        private readonly Func<Task> action;

        public SendRessourceManager(Func<Task> action)
        {
            this.action = action;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public async void Commit(Enlistment enlistment)
        {
            try
            {
                await this.action();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}