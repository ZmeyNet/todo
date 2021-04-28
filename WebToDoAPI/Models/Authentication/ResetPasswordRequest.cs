using System.ComponentModel.DataAnnotations;

namespace WebToDoAPI.Models.Authentication
{
    public class ResetPasswordRequest
    {
        [MinLength(3)]
        [MaxLength(128)]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Provided Email is not valid")]
        public string Email { get; set; }

        [MinLength(1)]
        [MaxLength(256)]
        [Required(ErrorMessage = "Reset Token is required")]
        public string ResetToken { get; set; }

        [MinLength(8)]
        [MaxLength(256)]
        [Required(ErrorMessage = "New Password is required")]
        public string NewPassword { get; set; }
    }
}
