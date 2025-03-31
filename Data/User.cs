using System.ComponentModel.DataAnnotations;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace Data
{
    /// <summary>
    /// Represents a user that has an account.
    /// </summary>
    [CollectionName("Users")]
    public class User : MongoIdentityUser<ObjectId>
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> OrdersIds { get; set; } = [];

        public List<CartItem> CartItems { get; set; } = [];

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> FavoriteBooksIds { get; set; } = [];
    }
}
