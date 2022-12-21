using JWT_API.Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace JWT_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
           
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserEntity>(entity => {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasMany(u => u.Roles).WithMany();
                entity.Property(u => u.EmailConfirmed).HasDefaultValue(false);
            });
 
        }
    }
}
