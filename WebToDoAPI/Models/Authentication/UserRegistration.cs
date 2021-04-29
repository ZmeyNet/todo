using System.ComponentModel.DataAnnotations;

namespace WebToDoAPI.Models.Authentication
{
    public class UserRegistration
    {
        [MinLength(1)]
        [MaxLength(128)]
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }

        [MinLength(3)]
        [MaxLength(256)]
        [EmailAddress(ErrorMessage ="Provided Email is invalid")]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
    }
}
