using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace todojuanes.Functions.Entities
{
    public class TodoEntity : TableEntity
    {
        public DateTime CreatedTime { get; set; }

        public string TaskDescription { get; set; }

        public bool isCompleted { get; set; }
    }
}
