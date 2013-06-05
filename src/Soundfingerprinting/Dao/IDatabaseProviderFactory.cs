namespace Soundfingerprinting.Dao
{
    using System.Data;

    public interface IDatabaseProviderFactory
    {
        IDbConnection CreateConnection();
    }
}
