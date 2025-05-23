using System.ComponentModel.DataAnnotations;
namespace Task_Management_System.Models.DTOs
{
    public class RegisterViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[\w\.-]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Email domain is not valid.")]
        public string Email { get; set; }


        [Required]
        public string Name { get; set; }

        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}