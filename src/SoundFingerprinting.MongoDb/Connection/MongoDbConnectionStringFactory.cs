namespace SoundFingerprinting.MongoDb.Connection
{
    using System;
    using System.Configuration;

    public class MongoDbConnectionStringFactory : IMongoDbConnectionStringFactory
    {
        public string GetConnectionString()
        {
            var mongoDbConnectionSection = GetMongoDbConnectionSection();

            return mongoDbConnectionSection.ConnectionString;
        }

        public string GetDatabaseName()
        {
            var mongoDbConnectionSection = GetMongoDbConnectionSection();

            return mongoDbConnectionSection.DatabaseName;
        }

        private MongoDbConnectionSection GetMongoDbConnectionSection()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            MongoDbConnectionSection mongoDbConnectionSection =
                config.GetSection("MongoDbConnectionSection") as MongoDbConnectionSection;

            if (mongoDbConnectionSection == null)
            {
                throw new Exception(
                    "MongoDbConnectionSection is not properly configured. No entry found in application configuration file for MongoDb management");
            }

            return mongoDbConnectionSection;
        }
    }
}