using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using OpenAI.Chat;
using SK.FunctionCalling.Plugins;
using System.Text.Json;
using System.Text;

namespace SK.FunctionCalling.Controllers
{
    public class BookController : Controller
    {
        private readonly Kernel _kernel;

        public BookController(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<IActionResult> Index()
        {
            // 创建 OrderBookPlugin 实例
            OrderBookPlugin orderBookPlugin = new OrderBookPlugin();

            // 获取书籍数据，使用 await 等待异步任务的完成
            var books = await orderBookPlugin.GetBookMenuAsync();
            // 将书籍数据传递到视图
            return View(books);
        }
        [HttpPost]
        [Route("Book/OrderBookAsync")]
        public async Task OrderBookAsync([FromBody] string prompt)
        {

            // 设置响应流的内容类型为 SSE
            HttpResponse response = HttpContext.Response;
            response.ContentType = "text/event-stream";  // 设置为 SSE
            response.Headers.Add("Cache-Control", "no-cache");  // 防止缓存
            response.Headers.Add("Connection", "keep-alive");  // 保持连接

            await response.StartAsync();  // 启动响应流

            string input_text = prompt;  // 使用传入的消息作为输入

            try
            {
                PromptExecutionSettings settings = new()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                };
                var functionResult = _kernel.InvokePromptStreamingAsync(
                   prompt,
                   new KernelArguments(settings)
                );
                await foreach (var update in functionResult)  // Correctly handle the IAsyncEnumerable
                {
                    var reply = update.ToString();

                    // Create and send SSE message
                    byte[] messageBytes = Encoding.UTF8.GetBytes(reply);
                    await response.Body.WriteAsync(messageBytes, 0, messageBytes.Length);
                    await response.Body.FlushAsync();  // Ensure data is sent immediately
                }
            }
            catch (Exception ex)
            {
                // 错误处理
                string errorMessage = $"data: {JsonSerializer.Serialize(new { error = "Error processing the request", details = ex.Message })}\n\n";
                await response.WriteAsync(errorMessage);
            }
            finally
            {
                // 确保响应结束
                await response.CompleteAsync();
            }
            // 显示结果
        }
    }
}
