using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Data
{
    /// <summary>
    /// Represents a book comment written by a user.
    /// </summary>
    [CollectionName("BookComments")]

    public class BookComment : Base
    {
        public string CommentText { get; set; } = string.Empty;

        public string User_Username { get; set; } = string.Empty;

        public DateTime TimePosted { get; set; } = DateTime.Now;

        [BsonRepresentation(BsonType.ObjectId)]
        public string User_Id { get; set; } = string.Empty;
    }
}
