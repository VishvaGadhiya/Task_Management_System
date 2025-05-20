using Microsoft.EntityFrameworkCore;
using Task_Management_System.Models;

namespace Task_Management_System.Data
{
    public class ApplicationDbContext : DbContext
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
    }
}
