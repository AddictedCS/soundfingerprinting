namespace SoundFingerprinting.Dao.Sqlite
{
    public class SqliteModelService : ModelService
    {
        public SqliteModelService(string configurationString)
            : base(new TrackDao(configurationString), null, null, null)
        {
            // no op
        }
    }
}
