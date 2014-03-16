namespace SoundFingerprinting.SQL
{
    using System.Data;

    public interface IDatabaseProviderFactory
    {
        IDbConnection CreateConnection();
    }
}