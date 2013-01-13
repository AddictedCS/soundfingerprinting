namespace Soundfingerprinting.Dao
{
    using System.Configuration;

    public class DefaultConnectionStringFactory : IConnectionStringFactory
    {
        public string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString;
        }
    }
}