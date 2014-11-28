namespace MMS.ServiceBus
{
    using System.Threading.Tasks;

    public interface IHandleMessageAsync<in TMessage>
    {
        Task Handle(TMessage message, IBus bus);
    }
}