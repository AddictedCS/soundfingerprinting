namespace SoundFingerprinting.SQL.ORM
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.SQL.Conditions;

    internal interface IExecutor
    {
        int AsNonQuery();

        T AsScalar<T>();

        T As<T>();

        T As<T>(Func<IReader, T> factory);

        TEntity AsModel<TEntity>(params ICondition<TEntity>[] conditions) where TEntity : new();

        IList<T> AsList<T>(Func<IReader, T> factory);

        IList<TEntity> AsListOfModel<TEntity>(params ICondition<TEntity>[] conditions) where TEntity : new();

        IDictionary<TKey, TValue> AsDictionary<TKey, TValue>(Func<IReader, TKey> keyFactory, Func<IReader, TValue> valueFactory);

        TEntity AsComplexModel<TEntity>(Action<TEntity, IReader> customFieldPopulator,  params ICondition<TEntity>[] conditions)
            where TEntity : new();

        IList<TEntity> AsListOfComplexModel<TEntity>(Action<TEntity, IReader> customFieldPopulator, params ICondition<TEntity>[] conditions)
            where TEntity : new();
    }
}