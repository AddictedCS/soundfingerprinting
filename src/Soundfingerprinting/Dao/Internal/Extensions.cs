namespace Soundfingerprinting.Dao.Internal
{
    using System.Data;
    using System.Data.Common;

    internal static class Extensions
    {
        public static DbCommand WithParameter<T>(this DbCommand command, string parameterName, T value, DbType databaseType)
        {
            DbParameter param = command.CreateParameter();
            param.ParameterName = parameterName;
            param.DbType = databaseType;
            param.Value = value;
            command.Parameters.Add(param);
            return command;
        }

        public static DbCommand WithConnection(this DbCommand command, DbConnection connection)
        {
            command.Connection = connection;
            return command;
        }
    }
}
