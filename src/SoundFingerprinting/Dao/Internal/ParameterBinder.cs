namespace SoundFingerprinting.Dao.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using SoundFingerprinting.Dao.Conditions;

    internal class ParameterBinder : IParameterBinder
    {
        private readonly IDbConnection connection;
        private readonly IDbCommand command;
        private readonly IModelBinderFactory modelBinderFactory;

        public ParameterBinder(IDbConnection connection, IDbCommand command, IModelBinderFactory modelBinderFactory)
        {
            this.connection = connection;
            this.command = command;
            this.modelBinderFactory = modelBinderFactory;
        }

        public IParameterBinder WithParameter(string name, string value)
        {
            CreateParameter(name, value, DbType.String);
            return this;
        }

        public IParameterBinder WithParameter(string name, int value)
        {
            CreateParameter(name, value, DbType.Int32);
            return this;
        }

        public IParameterBinder WithParameter(string name, DateTime value)
        {
            CreateParameter(name, value, DbType.DateTime);
            return this;
        }

        public IParameterBinder WithParameter<T>(string name, T value)
            where T : struct
        {
            CreateParameter(name, value, modelBinderFactory.GetParameterType<T>());
            return this;
        }

        public IParameterBinder WithParametersFromModel<TModel>(TModel model, params ICondition<TModel>[] conditions)
            where TModel : new()
        {
            IModelBinder<TModel> modelBinder = modelBinderFactory.Create(conditions);
            modelBinder.BindWriter(model, this);
            return this;
        }

        public void WithParameter(string name, DbType type, object value)
        {
            CreateParameter(name, value, type);
        }

        public IExecutor Execute()
        {
            return new Executor(connection, command, modelBinderFactory);
        }

        private void CreateParameter(string name, object value, DbType type)
        {
            try
            {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value ?? DBNull.Value;
                parameter.DbType = type;

                command.Parameters.Add(parameter);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        private void Dispose()
        {
            command.Dispose();
            connection.Dispose();
        }
    }
}