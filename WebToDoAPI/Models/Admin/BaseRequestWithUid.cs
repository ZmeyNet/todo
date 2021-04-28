using System.ComponentModel.DataAnnotations;

namespace WebToDoAPI.Models.Admin
{
    public class BaseRequestWithUid
    {
        [Required]
        [MinLength(30)]
        [MaxLength(70)]
        public string UserId { get; set; }
    }
}
