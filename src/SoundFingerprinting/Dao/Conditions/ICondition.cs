<<<<<<< HEAD
namespace SoundFingerprinting.Dao.Conditions
{
    using System;
    using System.Data;
    using System.Linq.Expressions;

    public interface ICondition<TModel>
    {
        string GetFullParameterName();

        Expression GetReaderTransformation(ParameterExpression parameterReader, string parameterName, Type propertyType);

        Expression GetWriterTransformation(Expression property);

        DbType GetParameterDbType();
    }
=======
namespace SoundFingerprinting.Dao.Conditions
{
    using System;
    using System.Data;
    using System.Linq.Expressions;

    public interface ICondition<TModel>
    {
        string GetFullParameterName();

        Expression GetReaderTransformation(ParameterExpression parameterReader, string parameterName, Type propertyType);

        Expression GetWriterTransformation(Expression property);

        DbType GetParameterDbType();
    }
>>>>>>> 29ad7f2255c9e65f055245321140987dbe9f1382
}