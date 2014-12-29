namespace MMS.ServiceBus
{
    public interface ITransactionalBusForHandlerProvider : ITransactionProvider
    {
        IBusForHandler Participate(ITransaction @in);
    }
}