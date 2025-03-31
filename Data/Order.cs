using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace Data
{
    /// <summary>
    /// Represents a placed order.
    /// </summary>
    [CollectionName("Orders")]
    public class Order : Base
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        public DateTime DateCreated { get; set; }

        public bool Sent { get; set; }

        public List<CartItem> Items { get; set; } = [];

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserBoughtId { get; set; } = string.Empty;
    }
}
