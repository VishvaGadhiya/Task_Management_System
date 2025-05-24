using System;
using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.Models.Dtos
{
    public class CreateOrEditUserDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
