using System.ComponentModel.DataAnnotations;

namespace WebToDoAPI.Models.Authentication
{
    public class LoginModel
    {
        [MinLength(3)]
        [MaxLength(128)]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Provided Email is not valid")]
        public string Email { get; set; }
        
        [MinLength(1)]
        [MaxLength(128)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
