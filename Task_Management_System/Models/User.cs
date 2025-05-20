namespace Task_Management_System.Models
{
    public class User
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime JoinDate { get; set; }
        public string Status { get; set; }

        public ICollection<UserTask>? UserTasks { get; set; }   


    }
}
