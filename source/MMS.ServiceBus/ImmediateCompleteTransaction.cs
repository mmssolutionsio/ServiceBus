namespace MMS.ServiceBus
{
    using System.Threading.Tasks;

    public class ImmediateCompleteTransaction : ITransactionEnlistment, ITransaction
    {
        public Task CompleteAsync()
        {
            return Task.FromResult(0);
        }

        public Task RollbackAsync()
        {
            return Task.FromResult(0);
        }

        public Task Enlist(ITransaction transaction)
        {
            return transaction.CompleteAsync();
        }
    }
}