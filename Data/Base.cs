using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Data
{
    /// <summary>
    /// Base class for MongoDB documents with an Id.
    /// </summary>
    public class Base
    {
        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
    }
}
