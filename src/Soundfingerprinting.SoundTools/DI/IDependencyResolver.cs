namespace Soundfingerprinting.SoundTools.DI
{
    using System;
    using System.Collections.Generic;

    using Ninject.Syntax;

    public interface IDependencyResolver
    {
        object GetService(Type serviceType);

        IEnumerable<object> GetServices(Type serviceType);

        IBindingToSyntax<T> Bind<T>();

        T Get<T>();
    }
}