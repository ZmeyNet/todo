using System.ComponentModel.DataAnnotations;

namespace WebToDoAPI.Models.Admin
{
    public class ChangeUserStatusRequest : BaseRequestWithUid
    {
        
        [Required]
        public bool IsDisabled { get; set; }
    }
}
