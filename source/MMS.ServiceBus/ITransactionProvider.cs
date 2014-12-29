namespace MMS.ServiceBus
{
    public interface ITransactionProvider
    {
        ITransaction BeginTransaction();
    }
}