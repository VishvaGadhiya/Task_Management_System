using Microsoft.AspNetCore.Identity;

namespace Task_Management_System.Models
{
    public class User : IdentityUser<int>
    {
        public string? Name { get; set; }
        public string? Gender { get; set; }
        public DateTime JoinDate { get; set; }
        public string? Status { get; set; }

        public ICollection<UserTask>? UserTasks { get; set; }   
    }
}
