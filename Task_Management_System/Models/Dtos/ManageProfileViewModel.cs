﻿using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.Models.DTOs
{
    public class ManageProfileViewModel
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; } 
    }

}
