using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Customer
    {
        [BsonId]
        [BsonElement("_id")]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("name")]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("active")]
        [Required(ErrorMessage = "Active status is required")]
        [Display(Name = "Is Active")]
        [DefaultValue(false)]
        public bool IsActive { get; set; } = false;
    }
}
