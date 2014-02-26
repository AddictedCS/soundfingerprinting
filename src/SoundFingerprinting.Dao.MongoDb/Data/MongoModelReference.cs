namespace SoundFingerprinting.Dao.MongoDb.Data
{
    using MongoDB.Bson;

    using SoundFingerprinting.DAO;

    public class MongoModelReference : ModelReference<ObjectId>
    {
        public MongoModelReference(ObjectId id)
            : base(id)
        {
        }
    }
}
