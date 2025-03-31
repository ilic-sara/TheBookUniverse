using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace Data
{
    /// <summary>
    /// Represents an author whose books are sold
    /// </summary>
    [CollectionName("Authors")]
    public class Author : Base
    {
        public string Name { get; set; } = string.Empty;

        public string About { get; set; } = string.Empty;

        public string PictureURL { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> BooksIds { get; set; } = [];
    }

}
