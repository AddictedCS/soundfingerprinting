namespace SoundFingerprinting.Infrastructure
{
    using System.Collections.Generic;

    public interface IDependencyResolver
    {
        T Get<T>();

        TInterface Get<TInterface>(IEnumerable<ConstructorArgument> constructorArguments);

        void Bind<TInterface, TImplementation>() where TImplementation : TInterface;

        void BindAsSingleton<TInterface, TImplementation>() where TImplementation : TInterface;

        void Bind<TInterface, TImplementation>(TImplementation constant) where TImplementation : TInterface;

        void BindAsSingleton<TInterface, TImplementation>(TImplementation constant)
            where TImplementation : TInterface;
    }
}
