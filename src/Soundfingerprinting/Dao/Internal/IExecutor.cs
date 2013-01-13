namespace Soundfingerprinting.Dao.Internal
{
    using System;
    using System.Collections.Generic;

    using Soundfingerprinting.Dao.Conditions;

    public interface IExecutor
    {
        int AsNonQuery();

        T AsScalar<T>();

        T As<T>();

        T As<T>(Func<IReader, T> factory);

        TEntity AsModel<TEntity>(params ICondition<TEntity>[] conditions) where TEntity : new();

        IList<T> AsList<T>(Func<IReader, T> factory);

        IList<TEntity> AsListOfModel<TEntity>(params ICondition<TEntity>[] conditions) where TEntity : new();

        IDictionary<TKey, TValue> AsDictionary<TKey, TValue>(Func<IReader, TKey> keyFactory, Func<IReader, TValue> valueFactory);
    }
}