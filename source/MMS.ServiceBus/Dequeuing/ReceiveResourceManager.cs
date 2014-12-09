//-------------------------------------------------------------------------------
// <copyright file="ReceiveResourceManager.cs" company="MMS AG">
//   Copyright (c) MMS AG, 2008-2015
// </copyright>
//-------------------------------------------------------------------------------

namespace MMS.ServiceBus.Dequeuing
{
    using System.Transactions;

    internal class ReceiveResourceManager : IEnlistmentNotification
    {
        private readonly TransportMessage receivedMessage;

        public ReceiveResourceManager(TransportMessage receivedMessage)
        {
            this.receivedMessage = receivedMessage;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            this.receivedMessage.Complete();

            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            this.receivedMessage.Abandon();

            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}