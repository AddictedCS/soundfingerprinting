namespace SoundFingerprinting.SQL
{
    using System.Configuration;

    public class DefaultConnectionStringFactory : IConnectionStringFactory
    {
        public string GetConnectionString()
        {
            if (ConfigurationManager.ConnectionStrings["FingerprintConnectionString"] != null)
            {
                return ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString;
            }

            return null;
        }
    }
}