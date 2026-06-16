using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class User
    {
        [BsonId]
        [BsonElement("_id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("username")]
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("email")]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("roles")]
        public List<Role> Roles { get; set; } = new();

    }
}
