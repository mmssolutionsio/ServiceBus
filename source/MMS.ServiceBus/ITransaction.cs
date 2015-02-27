namespace MMS.ServiceBus
{
    using System.Threading.Tasks;

    public interface ITransaction
    {
        Task CompleteAsync();

        Task RollbackAsync();
    }
}