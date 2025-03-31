using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;

namespace Data
{
    /// <summary>
    /// Represents a user role in the identity system.
    /// </summary>
    [CollectionName("Roles")]
    public class Role : MongoIdentityRole<ObjectId>
    {

    }
}
