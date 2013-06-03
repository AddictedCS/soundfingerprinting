namespace Soundfingerprinting.Infrastructure
{
    using System;
    using System.Collections.Generic;

    public interface IDependencyResolver
    {
        object GetService(Type serviceType);

        IEnumerable<object> GetServices(Type serviceType);

        T Get<T>();

        void Bind<TInterface, TImplementation>() where TImplementation : TInterface;

        void Bind<TInterface, TImplementation>(TImplementation constant) where TImplementation : TInterface;
    }
}
