<<<<<<< HEAD
﻿namespace SoundFingerprinting.Dao.Internal
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
=======
﻿namespace SoundFingerprinting.Dao.Internal
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
>>>>>>> 29ad7f2255c9e65f055245321140987dbe9f1382
