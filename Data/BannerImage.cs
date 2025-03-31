using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using MongoDbGenericRepository.Attributes;

namespace Data
{
    /// <summary>
    /// Represents a banner image displayed on the start page.
    /// </summary>
    [CollectionName("BannerImages")]

    public class BannerImage : Base
    {
        public string PictureURL { get; set; } = string.Empty;
    }
}
