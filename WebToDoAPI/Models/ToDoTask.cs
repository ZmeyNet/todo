using WebToDoAPI.Data.Entities;

namespace WebToDoAPI.Models
{
    public class ToDoTask
    {
        public ToDoTask() { }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        
        public ToDoTask(TaskEntity toDoTask)
        {
            Name = toDoTask.Name;
            Description = toDoTask.Description;
            Id = toDoTask.Id;
            IsCompleted = toDoTask.IsCompleted;
        }
    }
}
