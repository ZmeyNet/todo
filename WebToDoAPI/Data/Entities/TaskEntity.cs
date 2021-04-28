using System.ComponentModel.DataAnnotations;

namespace WebToDoAPI.Data.Entities
{
    public class TaskEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }

        public ApplicationUser User { get; set; }
    }
}