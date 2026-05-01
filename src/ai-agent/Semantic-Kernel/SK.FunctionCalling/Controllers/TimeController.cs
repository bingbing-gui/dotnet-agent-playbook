using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Functions; // 用于新版 KernelFunctionFactory
using OpenAI.Chat;
using SK.FunctionCalling.Models;

namespace SK.FunctionCalling.Controllers
{
    public class TimeController : Controller
    {

        private readonly Kernel _kernel;

        public TimeController(Kernel kernel)
        {
            _kernel = kernel;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Query(string city)
        {
            //var chat = new ChatHistory
            //{
            //    new Microsoft.SemanticKernel.ChatMessageContent(AuthorRole.User, $"{city}现在几点了?")
            //};
            var dic = new Dictionary<string, object>
            {
                { "city", $"{city}" }
            };

            PromptExecutionSettings settings = new()
            {
                ExtensionData = dic,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var prompt = $"查找{city}有名的饭店";
            var functionResult = await _kernel.InvokePromptAsync(
                prompt,
                new KernelArguments(settings)
            );
            return View("Index", functionResult);  // 显示结果
        }

    }
}
