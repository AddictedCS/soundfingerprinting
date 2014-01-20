namespace SoundFingerprinting.Infrastructure
{
    public interface IDependencyResolver
    {
        T Get<T>();

        TInterface Get<TInterface, T2>(ConstructorArgument<T2>[] constructorArguments);

        void Bind<TInterface, TImplementation>() where TImplementation : TInterface;

        void BindAsSingleton<TInterface, TImplementation>() where TImplementation : TInterface;

        void Bind<TInterface, TImplementation>(TImplementation constant) where TImplementation : TInterface;

        void BindAsSingleton<TInterface, TImplementation>(TImplementation constant)
            where TImplementation : TInterface;
    }
}
