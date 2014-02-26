namespace SoundFingerprinting.MongoDb.Connection
{
    using System.Configuration;

    public class MongoDbConnectionSection : ConfigurationSection
    {
        [ConfigurationProperty("connectionString")]
        public string ConnectionString
        {
            get
            {
                return (string)this["connectionString"];
            }

            set
            {
                this["connectionString"] = value;
            }
        }

        [ConfigurationProperty("databaseName")]
        public string DatabaseName
        {
            get
            {
                return (string)this["databaseName"];
            }

            set
            {
                this["databaseName"] = value;
            }
        }
    }
}
