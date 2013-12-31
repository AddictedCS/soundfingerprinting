namespace SoundFingerprinting.Dao.Internal
{
    using System;
    using System.Data;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;

    internal static class TypeRegistry
    {
        public static readonly Type ReaderType = typeof(IReader);

        public static readonly Type ParameterBinderType = typeof(IParameterBinder);

        public static readonly MethodInfo MethodGetRaw;

        public static readonly MethodInfo MethodWithParameter;

        public static readonly MethodInfo MethodGetEnumMemberByName;

        public static readonly MethodInfo MethodGetEnumMemberById;

        static TypeRegistry()
        {
            MethodGetRaw = GetMethod<IReader>(x => x.GetRaw);

            var types = new[] { typeof(string), typeof(DbType), typeof(object) };
            MethodWithParameter = GetMethod<IParameterBinder>(x => x.WithParameter, types);

            MethodGetEnumMemberByName = GetMethod<IReader>(x => x.GetEnumMemberByName<object>);
            MethodGetEnumMemberById = GetMethod<IReader>(x => x.GetEnumMemberById<object>);
        }

        private static MethodInfo GetMethod<T>(Expression<Func<T, Func<string, object>>> expression)
        {
            string methodName = GetMethodName(expression);
            return typeof(T).GetMethod(methodName);
        }

        private static MethodInfo GetMethod<T>(
            Expression<Func<T, Action<string, DbType, object>>> expression, Type[] types)
        {
            string methodName = GetMethodName(expression);
            return typeof(T).GetMethod(methodName, types);
        }

        /// <summary>
        /// Get method name from expression string representation
        /// </summary>
        /// <remarks>
        /// The regex inside will get the last "method" token 
        /// from string representation of expression. 
        /// Expression form was used to show corresponding method
        /// usages on static analysis.
        /// </remarks>
        /// <param name="expression">Expression to parse</param>
        /// <returns>Method name</returns>
        private static string GetMethodName(Expression expression)
        {
            var regex = new Regex(@".* (\w+)(?:\[.*?\])?\(.*?\)[^\w]*$");
            return regex.Match(expression.ToString()).Groups[1].Value;
        }
    }
}