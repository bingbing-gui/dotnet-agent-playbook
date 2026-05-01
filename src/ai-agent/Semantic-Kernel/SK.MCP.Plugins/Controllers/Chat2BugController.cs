using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SK.MCP.Plugins.Services;
using System.Text;
using System.Text.Json;

namespace SK.MCP.Plugins.Controllers
{
    public class Chat2BugController : Controller
    {
        private readonly IMcpToolService _mcpTools;

        private readonly Kernel _kernel;

        public Chat2BugController(IMcpToolService mcpTools, Kernel kernel)
        {
            _mcpTools = mcpTools;
            _kernel = kernel;
        }

        // GET /filesystem/tools
        [HttpGet("/chat2bug/index")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var names = await _mcpTools.ListToolNamesAsync(ct);
            return View("Index", names); // 简单传给视图
        }
        // POST /filesystem/ask  （模型可自动选择工具）
        [HttpPost("/chat2bug/ask")]
        public async Task Ask([FromBody] string prompt, CancellationToken ct)
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
                await foreach (var chunk in _mcpTools.AskWithAutoToolsAsync(prompt, ct))
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(chunk.ToString());
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
