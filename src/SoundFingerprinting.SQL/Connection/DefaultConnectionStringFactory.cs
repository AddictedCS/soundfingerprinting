namespace SoundFingerprinting.SQL.Connection
{
    using System.Configuration;

    internal class DefaultConnectionStringFactory : IConnectionStringFactory
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