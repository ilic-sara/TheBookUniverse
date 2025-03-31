using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace Data
{
    /// <summary>
    /// Represents a book available in the store.
    /// </summary>
    [CollectionName("Books")]
    public class Book : Base
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;

        public int PublishedYear { get; set; }

        public List<string> Genres { get; set; } = [];

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        public int NumberOfPages { get; set; }

        public int Inventory { get; set; }

        public string PictureURL { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string AuthorId { get; set; } = string.Empty;
    }
}
