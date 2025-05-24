using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.Models.DTOs
{
    // Add this to your Models/DTOs folder
    public class ChangeEmailViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New Email")]
        public string NewEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }
    }
}
