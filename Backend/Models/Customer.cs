using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    public class Customer
    {
        [BsonId]
        [BsonElement("_id")]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("active")]
        public bool IsActive { get; set; }
    }
}
