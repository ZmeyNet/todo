using System.ComponentModel.DataAnnotations;

namespace WebToDoAPI.Models
{
    public class ForgotPasswordRequest
    {
        [MinLength(3)]
        [MaxLength(128)]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Provided Email is not vaild")]
        public string Email { get; set; }
    }
}
