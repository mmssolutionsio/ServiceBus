namespace MMS.ServiceBus
{
    using System.Threading.Tasks;

    public interface ITransactionEnlistment
    {
        Task Enlist(ITransaction transaction);
    }
}