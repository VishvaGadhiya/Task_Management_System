using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task_Management_System.Models;

namespace Task_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        private readonly DbContextOptions options;

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            this.options = options;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }

        protected ApplicationDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var adminRole = new IdentityRole<int>
            {
                Id = 1,
                Name = "Admin",
                NormalizedName = "ADMIN"
            };

            var managerRole = new IdentityRole<int>
            {
                Id = 3,
                Name = "Manager",
                NormalizedName = "MANAGER"
            };

            var userRole = new IdentityRole<int>
            {
                Id = 2,
                Name = "User",
                NormalizedName = "USER"
            };

            modelBuilder.Entity<IdentityRole<int>>().HasData(adminRole, managerRole, userRole);

            var hasher = new PasswordHasher<User>();
            var adminUser = new User
            {
                Id = 1,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = "STATIC-ADMIN-SECURITY-STAMP",
                PasswordHash = hasher.HashPassword(new User(), "Admin@123"),
                Name = "Admin User",
                Gender = "Male",
                JoinDate = new DateTime(2024, 01, 01),
                Status = "Active"
            };

            modelBuilder.Entity<User>().HasData(adminUser);

            modelBuilder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int>
                {
                    UserId = 1,
                    RoleId = 1 // Admin role assigned to admin user
                }
            );
        }

    }
}
