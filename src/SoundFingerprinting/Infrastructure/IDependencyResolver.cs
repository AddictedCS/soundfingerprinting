namespace SoundFingerprinting.Infrastructure
{
    public interface IDependencyResolver
    {
        T Get<T>();
    }
}
