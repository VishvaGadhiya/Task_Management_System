using System;
using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.Models.DTOs
{
    public class ManagerViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
