using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.HttpClientWithHttpVerb.Models
{
    public class IndexModel
    {
        public List<TodoItem> CompleteTodoItems { get; set; }
        public List<TodoItem> IncompleteTodoItems { get; set; }

    }
}
