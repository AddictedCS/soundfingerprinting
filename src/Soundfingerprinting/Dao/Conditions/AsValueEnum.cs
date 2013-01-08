namespace Soundfingerprinting.Dao.Conditions
{
    using System;
    using System.Data;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using Soundfingerprinting.Dao.Internal;

    public class AsValueEnum<TModel> : ICondition<TModel>
    {
        private readonly Expression<Func<TModel, object>> expression;

        public AsValueEnum(Expression<Func<TModel, object>> expression)
        {
            this.expression = expression;
        }

        public string GetFullParameterName()
        {
            var regex = new Regex(@"((?:\.\w+)+)\W*$");
            return regex.Match(expression.ToString()).Groups[1].Value.Substring(1);
        }

        public Expression GetReaderTransformation(ParameterExpression parameterReader, string parameterName, Type propertyType)
        {
            Expression[] args = new Expression[] { Expression.Constant(parameterName) };
            MethodInfo memberByName = TypeRegistry.MethodGetEnumMemberById.MakeGenericMethod(propertyType);
            return Expression.Call(parameterReader, memberByName, args);
        }

        public Expression GetWriterTransformation(Expression property)
        {
            return Expression.Convert(property, typeof(int));
        }

        public DbType GetParameterDbType()
        {
            return DbType.Int32;
        }
    }
}