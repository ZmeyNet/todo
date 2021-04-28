using System.ComponentModel.DataAnnotations;

namespace WebToDoAPI.Models.Admin
{
    public class ChangeUserStatusRequest : BaseRequestWithUid
    {
        
        [Required]
        public bool UserActiveStatusToBeSet { get; set; }
    }
}
