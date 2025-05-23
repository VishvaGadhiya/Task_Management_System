using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.Models.DTOs
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
