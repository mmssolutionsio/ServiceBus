namespace MMS.ServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Transaction : ITransactionEnlistment, ITransaction
    {
        private readonly List<ITransaction> transactions = new List<ITransaction>();

        public async Task CompleteAsync()
        {
            foreach (var transaction in this.transactions)
            {
                await transaction.CompleteAsync();
            }
        }

        public async Task RollbackAsync()
        {
            foreach (var transaction in this.transactions)
            {
                await transaction.RollbackAsync();
            }
        }

        public Task Enlist(ITransaction transaction)
        {
            this.transactions.Add(transaction);

            return Task.FromResult(0);
        }
    }
}