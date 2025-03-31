using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Data
{
    /// <summary>
    /// Represents an item in shopping cart.
    /// </summary>
    public class CartItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string BookId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
