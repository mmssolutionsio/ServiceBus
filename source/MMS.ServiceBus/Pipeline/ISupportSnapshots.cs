namespace MMS.Common.ServiceBusWrapper.Pipeline
{
    public interface ISupportSnapshots
    {
        void TakeSnapshot();

        void DeleteSnapshot();
    }
}