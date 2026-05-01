using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
// Add references
using Azure.Identity;
using Azure.AI.Projects;
using Azure.AI.Inference;
using Azure;
using Azure.AI.OpenAI;
using System.Text.Json;
using ChatApp.Models;
using System.Text;
using Azure.Core;

namespace ChatApp.Controllers;
public class ChatController : Controller
{
    private readonly ILogger<ChatController> _logger;
    // 初始化聊天提示
    private List<ChatRequestMessage> prompt = new List<ChatRequestMessage>
        {
            new ChatRequestSystemMessage("你是个AI助手帮助回答问题.")
        };

    public ChatController(ILogger<ChatController> logger)
    {
        _logger = logger;
    }
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    [Route("Chat/SendMessageAsync")]  // API 路由
    public async Task SendMessageAsync([FromBody] string message)
    {
        // 设置响应流的内容类型为 SSE
        HttpResponse response = HttpContext.Response;
        response.ContentType = "text/event-stream";  // 设置为 SSE
        response.Headers.Add("Cache-Control", "no-cache");  // 防止缓存
        response.Headers.Add("Connection", "keep-alive");  // 保持连接
        await response.StartAsync();  // 启动响应流
        // 初始化配置
        IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        IConfigurationRoot configuration = builder.Build();
        string project_connection = configuration["PROJECT_CONNECTION"];
        string model_deployment = configuration["MODEL_DEPLOYMENT"];
        // 使用 ClientSecretCredential 代替 Azure CLI 认证
        string clientId = configuration["AZURE_CLIENT_ID"];
        string tenantId = configuration["AZURE_TENANT_ID"];
        string clientSecret = configuration["AZURE_CLIENT_SECRET"];
        // 使用 ClientSecretCredential 进行认证
        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        // 初始化项目客户端
        var projectClient = new AIProjectClient(project_connection, credential);
        // 获取聊天客户端
        ChatCompletionsClient chat = projectClient.GetChatCompletionsClient();
        // 初始化聊天提示
        var prompt = new List<ChatRequestMessage>
        {
            new ChatRequestSystemMessage("你是个AI助手帮助回答问题.")
        };
        string input_text = message;  // 使用传入的消息作为输入
        try
        {
            // 更新提示消息
            prompt.Add(new ChatRequestUserMessage(input_text));
            var requestOptions = new ChatCompletionsOptions
            {
                Model = model_deployment,
                Messages = prompt
            };
            // 获取聊天完成流式响应
            var chatResponse = await chat.CompleteStreamingAsync(requestOptions);
            // 处理流式响应
            await foreach (var update in chatResponse)  // 等待异步流返回数据
            {
                if (update.ContentUpdate != null)
                {
                    string reply = update.ContentUpdate;

                    // 创建并发送 SSE 消息
                    //string messageResponse = JsonSerializer.Serialize(new { reply = reply });
                    byte[] messageBytes = Encoding.UTF8.GetBytes(reply);
                    await response.Body.WriteAsync(messageBytes, 0, messageBytes.Length);
                    await response.Body.FlushAsync();  // 确保数据立即发送
                }
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
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
