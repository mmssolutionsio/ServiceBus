namespace MMS.ServiceBus
{
    internal static class TransactionEnlistmentExtension
    {
        public static ITransactionEnlistment AsEnlistment(this ITransaction transaction)
        {
            return (ITransactionEnlistment) transaction;
        }
    }
}