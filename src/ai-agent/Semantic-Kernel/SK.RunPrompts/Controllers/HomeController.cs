using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RunPrompts.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace RunPrompts.Controllers;

public class HomeController : Controller
{
    private readonly Kernel _kernel;

    private readonly IChatCompletionService _chatCompletionService;
    private readonly IChatHistoryCache _chatHistoryCache;
    public HomeController(Kernel kernel, IChatHistoryCache chatHistoryCache)
    {
        _kernel = kernel;
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        _chatHistoryCache = chatHistoryCache;
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

        HttpResponse response = HttpContext.Response;
        response.ContentType = "text/event-stream";  // 设置为 SSE
        response.Headers.Add("Cache-Control", "no-cache");  // 防止缓存
        response.Headers.Add("Connection", "keep-alive");  // 保持连接
        await response.StartAsync();  // 启动响应流
        var connectionId = HttpContext.Connection.Id;
        // 从缓存中获取或初始化聊天记录
        var chatHistory = _chatHistoryCache.GetOrCreate(connectionId);

        #region 创建一个语义内核提示模板
        //var skillMatch = Regex.Match(message, @"技能[:：](.*?)([;；]|$)");
        //var interestMatch = Regex.Match(message, @"兴趣[:：](.*)");
        //string skills = skillMatch.Success ? skillMatch.Groups[1].Value.Trim() : "";
        //string interests = interestMatch.Success ? interestMatch.Groups[1].Value.Trim() : "";

        // var skTemplateFactory = new KernelPromptTemplateFactory();
        // var skPromptTemplate = skTemplateFactory.Create(new PromptTemplateConfig(
        //     """
        //     你是一名乐于助人的职业顾问。请根据用户的技能和兴趣，推荐最多 5 个合适的职位角色。
        //     请以如下 JSON 格式返回内容：
        //     "职位推荐":
        //     {
        //     "recommendedRoles": [],          // 推荐的职位
        //     "industries": [],                // 所属行业
        //     "estimatedSalaryRange": ""       // 预计薪资范围
        //     }
        //     我的技能包括：{{$skills}}。我的兴趣包括：{{$interests}}。根据这些，哪些职位适合我？
        //     """
        // ));
        // // 渲染提示模板并传入参数
        // var skRenderedPrompt = await skPromptTemplate.RenderAsync(
        //    _kernel,
        //    new KernelArguments
        //    {
        //        ["skills"] = skills,
        //        ["interests"] = interests
        //    }
        //);
        // chatHistory.AddUserMessage(skRenderedPrompt);
        #endregion

        #region 创建Handlebars提示词模板
        var roleMatch = Regex.Match(message, @"角色[:：](.*?)([;；]|$)");
        var skillMatch = Regex.Match(message, @"技能[:：](.*)");
        string roles = roleMatch.Success ? roleMatch.Groups[1].Value.Trim() : "";
        string skill = skillMatch.Success ? skillMatch.Groups[1].Value.Trim() : "";
        var hbTemplateFactory = new HandlebarsPromptTemplateFactory();
        var hbPromptTemplate = hbTemplateFactory.Create(new PromptTemplateConfig()
        {
            TemplateFormat = "handlebars",
            Name = "MissingSkillsPrompt",
            Template = """
            <message role="system">
                指令：你是一名职业顾问。请分析用户当前技能与目标职位要求之间的技能差距。
            </message>         
            <message role="user">目标职位：{{targetRole}}</message>
            <message role="user">当前技能：{{currentSkills}}</message>
            <message role="assistant">
             “技能差距分析”：
             {
                 "缺失技能": [],
                 "建议学习的课程": [],
                 "推荐的认证": []
             }
             </message>
         """
        }
        );

        var hbRenderedPrompt = await hbPromptTemplate.RenderAsync(
        _kernel,
        new KernelArguments
        {
            ["targetRole"] = roles,
            ["currentSkills"] = skill
        });
        chatHistory.AddUserMessage(hbRenderedPrompt);
        #endregion
        ChatMessageContent reply = await _chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            kernel: _kernel
        );
        chatHistory.AddAssistantMessage(reply.ToString());
        Console.WriteLine("Assistant: " + reply.ToString());
        //// 创建并发送 SSE 消息
        string messageResponse = JsonSerializer.Serialize(new { reply = reply });
        byte[] messageBytes = Encoding.UTF8.GetBytes(reply.ToString());
        await response.Body.WriteAsync(messageBytes, 0, messageBytes.Length);
        await response.Body.FlushAsync();  // 确保数据立即发送

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
