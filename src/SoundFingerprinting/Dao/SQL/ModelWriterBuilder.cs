namespace SoundFingerprinting.Dao.SQL
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq.Expressions;
    using System.Reflection;

    using SoundFingerprinting.Dao.Conditions;

    internal class ModelWriterBuilder<TModel>
    {
        private readonly ParameterExpression parameterStoredProcedure;

        private readonly ParameterExpression parameterModel;

        private readonly ICollection<Expression> expressions;

        public ModelWriterBuilder()
        {
            parameterStoredProcedure = Expression.Parameter(TypeRegistry.ParameterBinderType, "storedProcedure");
            parameterModel = Expression.Parameter(typeof(TModel), "model");
            expressions = new Collection<Expression>();
        }

        public void AddProperty(
            string parameterName,
            string fullParameterName,
            DbType parameterType,
            LinkedList<PropertyInfo> propertiesInfo,
            IDictionary<string, ICondition<TModel>> transformations)
        {
            var property = GetMemberExpression(propertiesInfo);

            Expression p1 = Expression.Constant(parameterName);
            Expression p2 = Expression.Constant(parameterType);

            if (transformations.ContainsKey(fullParameterName))
            {
                property = transformations[fullParameterName].GetWriterTransformation(property);
            }
            else if (IsEnum(propertiesInfo.Last.Value))
            {
                property = Expression.Call(property, "ToString", Type.EmptyTypes);
            }

            Expression p3 = Expression.Convert(property, typeof(object));

            Expression call = Expression.Call(parameterStoredProcedure, TypeRegistry.MethodWithParameter, p1, p2, p3);
            expressions.Add(call);
        }

        public Action<IParameterBinder, TModel> Compile()
        {
            Expression body = Expression.Block(expressions);
            Expression<Action<IParameterBinder, TModel>> lambda =
                Expression.Lambda<Action<IParameterBinder, TModel>>(body, parameterStoredProcedure, parameterModel);
            return lambda.Compile();
        }

        private Expression GetMemberExpression(IEnumerable<PropertyInfo> propertiesInfo)
        {
            Expression property = null;

            foreach (var propertyInfo in propertiesInfo)
            {
                property = Expression.Property(property ?? parameterModel, propertyInfo);
            }

            return property;
        }

        private bool IsEnum(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.IsEnum;
        }
    }
}