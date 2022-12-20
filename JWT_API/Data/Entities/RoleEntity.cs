using System.ComponentModel.DataAnnotations;

namespace JWT_API.Data.Entities
{
    public class RoleEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
