namespace MMS.ServiceBus
{
    public interface IHandleMessage<in TMessage>
    {
        void Handle(TMessage message, IBus bus);
    }
}