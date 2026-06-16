using Contracts.DTO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Role
    {
        [BsonId]
        [BsonElement("_id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonRepresentation(BsonType.String)]
        public RoleType Type { get; set; } = RoleType.User;
    }
}
