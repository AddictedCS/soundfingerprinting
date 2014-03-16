namespace SoundFingerprinting.SQL.ORM
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using SoundFingerprinting.SQL.Conditions;

    internal class ModelReaderBuilder<TModel>
    {
        private readonly ParameterExpression parameterReader;

        private readonly ParameterExpression parameterModel;

        private readonly ICollection<Expression> expressions;

        public ModelReaderBuilder()
        {
            parameterReader = Expression.Parameter(TypeRegistry.ReaderType, "reader");
            parameterModel = Expression.Parameter(typeof(TModel), "model");

            expressions = new Collection<Expression>();
        }

        public void AddProperty(
            string parameterName,
            string fullParameterName,
            ICollection<PropertyInfo> propertiesInfo,
            IDictionary<string, ICondition<TModel>> transformations)
        {
            var propertyType = propertiesInfo.Last().PropertyType;

            Expression[] args = new Expression[] { Expression.Constant(parameterName) };
            var property = GetMemberExpression(propertiesInfo);

            Expression methodExpression;

            if (transformations.ContainsKey(fullParameterName))
            {
                methodExpression = transformations[fullParameterName].GetReaderTransformation(
                    parameterReader, parameterName, propertyType);
            }
            else if (IsEnumOrNullableEnum(propertyType))
            {
                MethodInfo memberByName = TypeRegistry.MethodGetEnumMemberByName.MakeGenericMethod(propertyType);
                methodExpression = Expression.Call(parameterReader, memberByName, args);
            }
            else
            {
                methodExpression = Expression.Call(parameterReader, TypeRegistry.MethodGetRaw, args);
            }

            Expression conversion = Expression.Convert(methodExpression, propertyType);
            Expression assignment = Expression.Assign(property, conversion);

            expressions.Add(assignment);
        }

        public void AddPropertyInitializer(ICollection<PropertyInfo> propertiesInfo)
        {
            if (propertiesInfo.Count() == 1)
            {
                PropertyInfo propertyInfo = propertiesInfo.Single();
                AddPropertyInitializer(propertiesInfo, propertyInfo);
            }
        }

        public Action<IReader, TModel> Compile()
        {
            Expression body = Expression.Block(expressions);
            Expression<Action<IReader, TModel>> lambda = Expression.Lambda<Action<IReader, TModel>>(
                body, parameterReader, parameterModel);
            return lambda.Compile();
        }

        private void AddPropertyInitializer(IEnumerable<PropertyInfo> propertiesInfo, PropertyInfo propertyInfo)
        {
            ConstructorInfo constructorInfo = propertyInfo.PropertyType.GetConstructor(Type.EmptyTypes);
            if (constructorInfo != null)
            {
                Expression instance = Expression.New(constructorInfo);
                var property = GetMemberExpression(propertiesInfo);
                Expression assignment = Expression.Assign(property, instance);

                expressions.Add(assignment);
            }
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

        private bool IsEnumOrNullableEnum(Type propertyType)
        {
            if (propertyType.IsEnum)
            {
                return true;
            }

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = Nullable.GetUnderlyingType(propertyType);
            }

            return propertyType.IsEnum;
        }
    }
}