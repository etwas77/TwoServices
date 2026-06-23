using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;

namespace Backend.Models
{
    public class Item
    {
        [BsonId]
        [BsonElement("_id")]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("quantity")]
        [DefaultValue(0)]
        public decimal Quantity { get; set; }
    }
}
