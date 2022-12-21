using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JWT_API.Data.Entities
{

    public class UserEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(30)]
        public string LastName { get; set; }
        [Required]
        [MaxLength(50)]
        public string Email { get; set; }
        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [JsonIgnore]
        [Required]
        public string PasswordHash { get; set; }

        public bool EmailConfirmed { get; set; }
        
        public ICollection<RoleEntity> Roles { get; set; }
    }
}
