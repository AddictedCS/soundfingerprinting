namespace SoundFingerprinting.Command
{
    public interface IUsingRealtimeQueryServices
    {
        IRealtimeQueryCommand UsingServices(IModelService modelService);
    }
}