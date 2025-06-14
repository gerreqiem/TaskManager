using System.Collections.Generic;
namespace TaskManagerApp.Models
{
    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}