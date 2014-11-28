using System.Threading.Tasks;

namespace MMS.ServiceBus.Pipeline
{
    public class SyncAsAsyncHandlerDecorator<TMessage> : IHandleMessageAsync<TMessage>
    {
        private readonly IHandleMessage<TMessage> handler;

        public SyncAsAsyncHandlerDecorator(IHandleMessage<TMessage> handler)
        {
            this.handler = handler;
        }

        public async Task Handle(TMessage message, IBus bus)
        {
            await Task.Run(() => this.handler.Handle(message, bus)).ConfigureAwait(false);
        }
    }
}