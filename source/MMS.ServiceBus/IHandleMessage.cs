namespace MMS.Common.ServiceBusWrapper
{
    using System.Threading.Tasks;

    public interface IHandleMessageAsync<in TMessage>
    {
        Task Handle(TMessage message, IBus bus);
    }

    public interface IHandleMessage<in TMessage>
    {
        void Handle(TMessage message, IBus bus);
    }
}