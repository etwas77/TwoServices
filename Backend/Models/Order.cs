using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    public class Order
    {
        [BsonId]
        [BsonElement("_id")]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [BsonElement("items")]
        public List<Item> Items { get; set; } = new();
    }
}
