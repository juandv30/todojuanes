using System;

namespace todojuanes.Common.Models
{
    public class Todo
    {
        public DateTime CreatedTime { get; set; }

        public string TaskDescription { get; set; }

        public bool isCompleted { get; set; }
    }
}
