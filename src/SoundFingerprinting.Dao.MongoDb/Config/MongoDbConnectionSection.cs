namespace SoundFingerprinting.Dao.MongoDb.Config
{
    using System.Configuration;

    public class MongoDbConnectionSection : ConfigurationSection
    {
        [ConfigurationProperty("ConnectionString")]
        public ConfigurationElement ConnectionString
        {
            get { return (ConfigurationElement)this["connectionString"]; }
            set { this["connectionString"] = value; }
        }
    }
}
