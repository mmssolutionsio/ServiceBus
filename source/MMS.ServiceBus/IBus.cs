namespace MMS.ServiceBus
{
    using System.Threading.Tasks;

    public interface IBus
    {
        Task Send(object message, SendOptions options = null);

        void DoNotContinueDispatchingCurrentMessageToHandlers();
    }
}