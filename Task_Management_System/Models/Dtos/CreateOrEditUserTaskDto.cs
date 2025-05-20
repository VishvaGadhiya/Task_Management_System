namespace Task_Management_System.Models.DTOs
{
    public class CreateOrEditUserTaskDto
    {
        public int Id { get; set; }  
        public int UserId { get; set; }
        public int TaskId { get; set; }
    }
}
