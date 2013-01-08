namespace Soundfingerprinting.Dao
{
    using System.Data;
    using System.Data.Common;

    public interface IDatabaseProviderFactory
    {
        IDbConnection CreateConnection();
    }
}
