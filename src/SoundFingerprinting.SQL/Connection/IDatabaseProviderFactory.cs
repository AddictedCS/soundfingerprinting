namespace SoundFingerprinting.SQL.Connection
{
    using System.Data;

    internal interface IDatabaseProviderFactory
    {
        IDbConnection CreateConnection();
    }
}