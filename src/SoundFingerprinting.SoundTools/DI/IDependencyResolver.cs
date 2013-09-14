namespace SoundFingerprinting.SoundTools.DI
{
    using System;
    using System.Collections.Generic;

    using Ninject.Syntax;

    public interface IDependencyResolver
    {
        object GetService(Type serviceType);

        IEnumerable<object> GetServices(Type serviceType);

        IBindingToSyntax<T> Bind<T>();

        IBindingToSyntax<T1, T2> Bind<T1, T2>();

        IBindingToSyntax<T1, T2, T3> Bind<T1, T2, T3>();

        T Get<T>();
    }
}