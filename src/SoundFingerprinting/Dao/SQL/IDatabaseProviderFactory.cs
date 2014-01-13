namespace SoundFingerprinting.Dao.SQL
{
    using System.Data;

    public interface IDatabaseProviderFactory
    {
        IDbConnection CreateConnection();
    }
}