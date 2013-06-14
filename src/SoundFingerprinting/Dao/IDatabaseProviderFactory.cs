namespace SoundFingerprinting.Dao
{
    using System.Data;

    public interface IDatabaseProviderFactory
    {
        IDbConnection CreateConnection();
    }
}
