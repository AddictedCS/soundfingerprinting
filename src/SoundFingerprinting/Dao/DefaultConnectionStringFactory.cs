<<<<<<< HEAD
namespace SoundFingerprinting.Dao
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
=======
namespace SoundFingerprinting.Dao
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
>>>>>>> 29ad7f2255c9e65f055245321140987dbe9f1382
}