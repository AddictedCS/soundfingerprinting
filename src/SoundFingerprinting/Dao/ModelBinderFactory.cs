namespace SoundFingerprinting.Dao
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using SoundFingerprinting.Dao.Conditions;
    using SoundFingerprinting.Dao.Internal;

    public class ModelBinderFactory : IModelBinderFactory
    {
        private readonly Dictionary<Type, DbType> databaseTypes = new Dictionary<Type, DbType>
            {
                { typeof(string), DbType.String },
                { typeof(bool), DbType.Boolean },
                { typeof(float), DbType.Single },
                { typeof(double), DbType.Double },
                { typeof(byte), DbType.Byte },
                { typeof(sbyte), DbType.SByte },
                { typeof(short), DbType.Int16 },
                { typeof(ushort), DbType.UInt16 },
                { typeof(int), DbType.Int32 },
                { typeof(uint), DbType.UInt32 },
                { typeof(long), DbType.Int64 },
                { typeof(ulong), DbType.UInt64 },
                { typeof(DateTime), DbType.DateTime },
                { typeof(char), DbType.StringFixedLength },
                { typeof(decimal), DbType.Decimal },
                { typeof(Guid), DbType.Guid },
                { typeof(TimeSpan), DbType.Time },
                { typeof(Enum), DbType.String },
                { typeof(byte[]), DbType.Binary }
            };
        
        public IModelBinder<TModel> Create<TModel>(params ICondition<TModel>[] conditions) where TModel : new()
        {
            var modelType = typeof(TModel);

            var readerBuilder = new ModelReaderBuilder<TModel>();
            var writerBuilder = new ModelWriterBuilder<TModel>();

            var parentProperties = new LinkedList<PropertyInfo>();

            var conditionMap = conditions.ToDictionary(x => x.GetFullParameterName(), x => x);

            Create(modelType, string.Empty, readerBuilder, writerBuilder, parentProperties, conditionMap);

            return new ModelBinder<TModel>(readerBuilder.Compile(), writerBuilder.Compile());
        }

        public DbType GetParameterType<T>()
        {
            return databaseTypes[typeof(T)];
        }

        private void Create<TModel>(
            Type modelType,
            string prefix,
            ModelReaderBuilder<TModel> readerBuilder,
            ModelWriterBuilder<TModel> writerBuilder,
            LinkedList<PropertyInfo> parentProperties,
            IDictionary<string, ICondition<TModel>> conditions)
            where TModel : new()
        {
            var properties = modelType.GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IgnoreBindingAttribute)))
                {
                    continue;
                }

                bool isPropStatic = (propertyInfo.CanRead && propertyInfo.GetGetMethod().IsStatic) || (propertyInfo.CanWrite && propertyInfo.GetSetMethod().IsStatic);

                if (isPropStatic)
                {
                    continue;
                }

                var subParentProperties = new LinkedList<PropertyInfo>(parentProperties);
                subParentProperties.AddLast(propertyInfo);

                string fullParameterName = GetFullParameterName(subParentProperties);
                string parameterName = GetParameterName(prefix, propertyInfo);
                DbType parameterDbType = GetParameterDbType(propertyInfo, fullParameterName, conditions);

                if (parameterDbType != DbType.Object)
                {
                    readerBuilder.AddProperty(parameterName, fullParameterName, subParentProperties, conditions);
                    writerBuilder.AddProperty(parameterName, fullParameterName, parameterDbType, subParentProperties, conditions);
                }
                else
                {
                    var subPrefix = GetSubPrefix(prefix, propertyInfo);
                    readerBuilder.AddPropertyInitializer(subParentProperties);

                    Create(propertyInfo.PropertyType, subPrefix, readerBuilder, writerBuilder, subParentProperties, conditions);
                }
            }
        }

        private string GetSubPrefix(string prefix, PropertyInfo propertyInfo)
        {
            return prefix + propertyInfo.Name + "_";
        }

        private DbType GetParameterDbType<TModel>(PropertyInfo propertyInfo, string fullParameterName, IDictionary<string, ICondition<TModel>> conditions)
        {
            if (conditions.ContainsKey(fullParameterName))
            {
                return conditions[fullParameterName].GetParameterDbType();
            }

            var propertyType = propertyInfo.PropertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = Nullable.GetUnderlyingType(propertyType);
            }

            DbType type;
            if (databaseTypes.TryGetValue(propertyType, out type))
            {
                return type;
            }

            if (propertyType.IsEnum)
            {
                return DbType.String;
            }

            return DbType.Object;
        }

        private string GetParameterName(string prefix, PropertyInfo propertyInfo)
        {
            return prefix + propertyInfo.Name;
        }

        private string GetFullParameterName(IEnumerable<PropertyInfo> propertiesInfo)
        {
            return string.Join(".", propertiesInfo.Select(x => x.Name));
        }
    }
}