using AspNetCore.HttpClientWithHttpVerb.Models;
using AspNetCore.UsingHttpVerb.Practice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.HttpClientWithHttpVerb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TodoClient _todoClient;
        private readonly IHttpClientFactory _httpClientFactory;
        //private readonly IOperationScoped _operationScoped;
        public HomeController(ILogger<HomeController> logger,
           TodoClient todoClient,
           IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _todoClient = todoClient;
            _httpClientFactory = httpClientFactory;
        }
        /// <summary>
        /// 调用HttpClient Get
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var todoItems = await _todoClient.GetItemsAsync();
            IndexModel indexModel = new IndexModel();
            var completeTodoItems = todoItems.Where(x => x.IsComplete).ToList();
            var incompleteTodoItems = todoItems.Except(completeTodoItems).ToList();
            indexModel.CompleteTodoItems = completeTodoItems;
            indexModel.IncompleteTodoItems = incompleteTodoItems;
            return View(indexModel);
        }
        /// <summary>
        /// Create Item
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<IActionResult> CreateAsync([Required] string name)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            var newTodoItem = new TodoItem
            {
                Name = name
            };
            await _todoClient.CreateItemAsync(newTodoItem);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> EditAsync(long id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            var todoItem = await _todoClient.GetItemAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }
            return View(todoItem);
        }
        public async Task<IActionResult> SaveAsync(TodoItem todoItem)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            await _todoClient.SaveItemAsync(todoItem);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteAsync(long id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            await _todoClient.DeleteItemAsync(id);

            return RedirectToAction(nameof(Index));
        }
        //public async Task<IActionResult> Operation()
        //{
        //    var httpClient=_httpClientFactory.CreateClient("operation");
        //    var operationIdFromRequestScope = _operationScoped.OperationId;
        //    var operationIdFromHandlerScope = await httpClient.GetStringAsync("https://example.com");
        //    var operationModel = new OperationModel()
        //    { 
        //        OperationIdFromRequestScope= operationIdFromRequestScope,
        //        OperationIdFromHandlerScope = operationIdFromHandlerScope
        //    };
        //    return View(operationModel);
        //}
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
