using AspNetCore.FormatResponseOutputData.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.FormatResponseOutputData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoItemStore _todoItemStore;

        public TodoItemsController(TodoItemStore todoItemStore)
            => _todoItemStore = todoItemStore;

        [HttpGet]
        public IActionResult Get()
            => Ok(_todoItemStore.GetList());
        [HttpGet("{id:long}")]
        public IActionResult GetById(long id)
        {
            var todo = _todoItemStore.GetById(id);

            if (todo is null)
            {
                return NotFound();
            }

            return Ok(todo);
        }

        [HttpGet("poco/{id:long}")]
        public TodoItem? GetPOCOById(long id)
                => _todoItemStore.GetById(id);

        [HttpGet("Version")]
        public ContentResult GetVersion()
            => Content("v1.0.0");

        [HttpGet("Error")]
        public IActionResult GetError()
            => Problem("Something went wrong.");
    }
}