using System.ComponentModel.DataAnnotations;

namespace WebToDoAPI.Models
{
    public class LoginModel
    {
        [MinLength(1)]
        [MaxLength(128)]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Provided Email is not vaild")]
        public string Email { get; set; }
        
        [MinLength(1)]
        [MaxLength(128)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
