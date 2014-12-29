namespace MMS.ServiceBus
{
    public interface ITransactionalBusProvider : ITransactionProvider
    {
        IBus Participate(ITransaction @in);
    }
}