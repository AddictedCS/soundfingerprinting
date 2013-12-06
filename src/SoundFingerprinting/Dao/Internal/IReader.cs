<<<<<<< HEAD
namespace SoundFingerprinting.Dao.Internal
{
    using System;

    public interface IReader
    {
        string GetString(string name);

        bool GetBoolean(string name);

        short GetInt16(string name);

        int GetInt32(string name);

        long GetInt64(string name);

        decimal GetDecimal(string name);

        DateTime GetDateTime(string name);

        T GetEnumMemberById<T>(string name);

        T GetEnumMemberByName<T>(string name);

        object GetRaw(string name);
    }
=======
namespace SoundFingerprinting.Dao.Internal
{
    using System;

    public interface IReader
    {
        string GetString(string name);

        bool GetBoolean(string name);

        short GetInt16(string name);

        int GetInt32(string name);

        long GetInt64(string name);

        decimal GetDecimal(string name);

        DateTime GetDateTime(string name);

        T GetEnumMemberById<T>(string name);

        T GetEnumMemberByName<T>(string name);

        object GetRaw(string name);
    }
>>>>>>> 29ad7f2255c9e65f055245321140987dbe9f1382
}