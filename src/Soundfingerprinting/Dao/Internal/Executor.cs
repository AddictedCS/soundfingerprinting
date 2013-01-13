namespace Soundfingerprinting.Dao.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Soundfingerprinting.Dao.Conditions;

    internal class Executor : IExecutor
    {
        private readonly IDbConnection connection;
        private readonly IDbCommand command;
        private readonly IModelBinderFactory modelBinderFactory;

        public Executor(IDbConnection connection, IDbCommand command, IModelBinderFactory modelBinderFactory)
        {
            this.connection = connection;
            this.command = command;
            this.modelBinderFactory = modelBinderFactory;
        }

        public int AsNonQuery()
        {
            return SafeExec(ExecuteNonQuery);
        }

        public T AsScalar<T>()
        {
            return SafeExec(
                () => (T)ExecuteScalar());
        }

        public T As<T>()
        {
            return SafeExec(() => (T)ExecuteScalar());
        }

        public T As<T>(Func<IReader, T> factory)
        {
            return SafeExec(() =>
                {
                    var reader = ExecuteReader();

                    if (reader.Read())
                    {
                        T entity = factory(reader);
                        return entity;
                    }

                    return default(T);
                });
        }

        public TEntity AsModel<TEntity>(params ICondition<TEntity>[] conditions)
            where TEntity : new()
        {
            return SafeExec(() =>
                {
                    IModelBinder<TEntity> modelBinder = modelBinderFactory.Create(conditions);
                    var reader = ExecuteReader();

                    if (reader.Read())
                    {
                        TEntity entity = modelBinder.BindReader(reader);
                        return entity;
                    }

                    return default(TEntity);
                });
        }

        public IList<T> AsList<T>(Func<IReader, T> factory)
        {
            return SafeExec(() =>
                {
                    var reader = ExecuteReader();
                    var result = new List<T>();

                    while (reader.Read())
                    {
                        T entity = factory(reader);
                        result.Add(entity);
                    }

                    return result;
                });
        }

        public IList<TEntity> AsListOfModel<TEntity>(params ICondition<TEntity>[] conditions)
            where TEntity : new()
        {
            return SafeExec(() =>
                {
                    IModelBinder<TEntity> modelBinder = modelBinderFactory.Create(conditions);
                    Reader reader = ExecuteReader();
                    var result = new List<TEntity>();

                    while (reader.Read())
                    {
                        TEntity entity = modelBinder.BindReader(reader);
                        result.Add(entity);
                    }

                    return result;
                });
        }

        public IDictionary<TKey, TValue> AsDictionary<TKey, TValue>(Func<IReader, TKey> keyFactory, Func<IReader, TValue> valueFactory)
        {
            return SafeExec(() =>
                {
                    var reader = ExecuteReader();
                    var result = new Dictionary<TKey, TValue>();

                    while (reader.Read())
                    {
                        TKey key = keyFactory(reader);
                        TValue value = valueFactory(reader);
                        result.Add(key, value);
                    }

                    return result;
                });
        }

        private T SafeExec<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            finally
            {
                Dispose();
            }
        }

        private int ExecuteNonQuery()
        {
            command.Connection.Open();
            return command.ExecuteNonQuery();
        }

        private Reader ExecuteReader()
        {
            command.Connection.Open();
            return new Reader(command.ExecuteReader());
        }

        private object ExecuteScalar()
        {
            command.Connection.Open();
            return command.ExecuteScalar();
        }

        private void Dispose()
        {
            command.Dispose();
            connection.Dispose();
        }
    }
}