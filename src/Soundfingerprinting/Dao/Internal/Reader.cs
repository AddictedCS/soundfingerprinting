namespace Soundfingerprinting.Dao.Internal
{
    using System;
    using System.Data;

    internal class Reader : IReader
    {
        private readonly IDataReader dataReader;

        public Reader(IDataReader dataReader)
        {
            this.dataReader = dataReader;
        }

        public string GetString(string name)
        {
            int ordinal = dataReader.GetOrdinal(name);
            return dataReader.IsDBNull(ordinal) ? string.Empty : dataReader.GetString(ordinal);
        }

        public bool GetBoolean(string name)
        {
            int ordinal = dataReader.GetOrdinal(name);
            return dataReader.IsDBNull(ordinal) ? default(bool) : dataReader.GetBoolean(ordinal);
        }

        public short GetInt16(string name)
        {
            int ordinal = dataReader.GetOrdinal(name);
            return dataReader.IsDBNull(ordinal) ? default(short) : dataReader.GetInt16(ordinal);
        }

        public int GetInt32(string name)
        {
            int ordinal = dataReader.GetOrdinal(name);
            return dataReader.IsDBNull(ordinal) ? default(int) : dataReader.GetInt32(ordinal);
        }

        public long GetInt64(string name)
        {
            int ordinal = dataReader.GetOrdinal(name);
            return dataReader.IsDBNull(ordinal) ? default(long) : dataReader.GetInt64(ordinal);
        }

        public DateTime GetDateTime(string name)
        {
            int ordinal = dataReader.GetOrdinal(name);
            return dataReader.IsDBNull(ordinal) ? default(DateTime) : dataReader.GetDateTime(ordinal);
        }

        public decimal GetDecimal(string name)
        {
            int ordinal = dataReader.GetOrdinal(name);
            return dataReader.IsDBNull(ordinal) ? default(decimal) : dataReader.GetDecimal(ordinal);
        }

        public T GetEnumMemberById<T>(string name)
        {
            int id = GetInt32(name);
            return (T)Enum.ToObject(typeof(T), id);
        }

        public T GetEnumMemberByName<T>(string name)
        {
            int ordinal = dataReader.GetOrdinal(name);

            if (dataReader.IsDBNull(ordinal))
            {
                return default(T);
            }

            var t = typeof(T);
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                t = Nullable.GetUnderlyingType(t);
            }

            return (T)Enum.Parse(t, dataReader.GetString(ordinal), true);
        }

        public object GetRaw(string name)
        {
            int ordinal = dataReader.GetOrdinal(name);

            if (dataReader.IsDBNull(ordinal))
            {
                return null;
            }

            return dataReader.GetValue(ordinal);
        }

        public bool Read()
        {
            return dataReader.Read();
        }
    }
}