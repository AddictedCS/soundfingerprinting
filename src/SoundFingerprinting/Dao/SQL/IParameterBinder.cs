namespace SoundFingerprinting.Dao.SQL
{
    using System;
    using System.Data;

    using SoundFingerprinting.Dao.SQL.Conditions;

    public interface IParameterBinder
    {
        IParameterBinder WithParameter(string name, string value);

        IParameterBinder WithParameter(string name, int value);

        IParameterBinder WithParameter<T>(string name, T value);

        IParameterBinder WithParameter(string name, DateTime value);

        IParameterBinder WithParametersFromModel<TModel>(TModel model, params ICondition<TModel>[] conditions)
            where TModel : new();

        void WithParameter(string name, DbType type, object value);

        IExecutor Execute();
    }
}