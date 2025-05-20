using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Management_System.Models
{
    public class UserTask
    {
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public User? User { get; set; }

        [ForeignKey("TaskId")]
        public int TaskId { get; set; }
        public Tasks? Task { get; set; }
    }
}
