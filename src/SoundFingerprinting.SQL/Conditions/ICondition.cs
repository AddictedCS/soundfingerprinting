namespace SoundFingerprinting.SQL.Conditions
{
    using System;
    using System.Data;
    using System.Linq.Expressions;

    internal interface ICondition<TModel>
    {
        string GetFullParameterName();

        Expression GetReaderTransformation(ParameterExpression parameterReader, string parameterName, Type propertyType);

        Expression GetWriterTransformation(Expression property);

        DbType GetParameterDbType();
    }
}