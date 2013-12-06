<<<<<<< HEAD
namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;
    using System.Data;

    using SoundFingerprinting.Dao.Conditions;
    using SoundFingerprinting.Infrastructure;

    public class CachedModelBinderFactory : IModelBinderFactory
    {
        private readonly IModelBinderFactory modelBinderFactory;
        private readonly Dictionary<string, object> cache = new Dictionary<string, object>();

        public CachedModelBinderFactory()
            : this(DependencyResolver.Current.Get<IModelBinderFactory>())
        {
        }

        public CachedModelBinderFactory(IModelBinderFactory modelBinderFactory)
        {
            this.modelBinderFactory = modelBinderFactory;
        }

        public IModelBinder<TModel> Create<TModel>(params ICondition<TModel>[] conditions) where TModel : new()
        {
            if (cache.ContainsKey(typeof(TModel).FullName))
            {
                return (IModelBinder<TModel>)cache[typeof(TModel).FullName];
            }
            
            var modelBinder = modelBinderFactory.Create(conditions);
            cache[typeof(TModel).FullName] = modelBinder;
            return modelBinder;
        }

        public DbType GetParameterType<T>() where T : struct
        {
            return modelBinderFactory.GetParameterType<T>();
        }
    }
=======
namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;
    using System.Data;

    using SoundFingerprinting.Dao.Conditions;
    using SoundFingerprinting.Infrastructure;

    public class CachedModelBinderFactory : IModelBinderFactory
    {
        private readonly IModelBinderFactory modelBinderFactory;
        private readonly Dictionary<string, object> cache = new Dictionary<string, object>();

        public CachedModelBinderFactory()
            : this(DependencyResolver.Current.Get<IModelBinderFactory>())
        {
        }

        public CachedModelBinderFactory(IModelBinderFactory modelBinderFactory)
        {
            this.modelBinderFactory = modelBinderFactory;
        }

        public IModelBinder<TModel> Create<TModel>(params ICondition<TModel>[] conditions) where TModel : new()
        {
            if (cache.ContainsKey(typeof(TModel).FullName))
            {
                return (IModelBinder<TModel>)cache[typeof(TModel).FullName];
            }
            
            var modelBinder = modelBinderFactory.Create(conditions);
            cache[typeof(TModel).FullName] = modelBinder;
            return modelBinder;
        }

        public DbType GetParameterType<T>() where T : struct
        {
            return modelBinderFactory.GetParameterType<T>();
        }
    }
>>>>>>> 29ad7f2255c9e65f055245321140987dbe9f1382
}