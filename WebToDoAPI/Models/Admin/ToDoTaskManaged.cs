using WebToDoAPI.Data.Entities;

namespace WebToDoAPI.Models.Admin
{
    public class ToDoTaskManaged : ToDoTask
    {
        private string UserId { get; set; }

        public ToDoTaskManaged(TaskEntity toDoTask):base(toDoTask)
        {
            this.UserId = toDoTask.User.Id;
        }
    }
}