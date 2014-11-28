namespace MMS.ServiceBus.Pipeline
{
    public interface ISupportSnapshots
    {
        void TakeSnapshot();

        void DeleteSnapshot();
    }
}