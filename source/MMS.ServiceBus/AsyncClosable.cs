namespace MMS.ServiceBus
{
    using System;
    using System.Threading.Tasks;

    public class AsyncClosable
    {
        private readonly Func<Task> closable;

        public AsyncClosable(Func<Task> closable)
        {
            this.closable = closable;
        }

        public Task CloseAsync()
        {
            return this.closable();
        }
    }
}