namespace WebToDoAPI.Models.Admin
{
    public class DeleteUserTaskRequest : BaseRequestWithUid
    {
        public int TaskId { get; set; }
    }
}