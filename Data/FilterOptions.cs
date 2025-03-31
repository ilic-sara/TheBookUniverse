using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using MongoDbGenericRepository.Attributes;

namespace Data
{
    /// <summary>
    /// Represents filter options that admin users can configure.
    /// </summary>
    [CollectionName("FilterOptions")]

    public class FilterOptions : Base
    {
        public string Name { get; set; } = string.Empty;

        public List<string> Values { get; set; } = [];

    }
}
