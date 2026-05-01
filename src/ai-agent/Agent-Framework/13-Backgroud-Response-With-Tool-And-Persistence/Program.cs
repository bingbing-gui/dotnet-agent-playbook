//// Copyright (c) Microsoft. All rights reserved.

//// This sample demonstrates how to use background responses with ChatClientAgent and Azure OpenAI Responses for long-running operations.
//// It shows polling for completion using continuation tokens, function calling during background operations,
//// and persisting/restoring agent state between polling cycles.

using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

var stateStore = new Dictionary<string, JsonElement?>();

//ResponsesClient仅用于评估目的，未来更新中可能会更改或移除。若要继续，请忽略/抑制此诊断提示。
#pragma warning disable OPENAI001 
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
     .GetResponsesClient(deploymentName)
     .CreateAIAgent(
        name: "SpaceNovelWriter",
        instructions: @"你是一名太空题材小说作家。
在写作之前，始终先研究相关的真实背景资料，并为主要角色生成角色设定。
写作时直接完成完整章节，不要请求批准或反馈。
不要向用户询问语气、风格、节奏或格式偏好——只需根据请求直接创作小说。",
        tools: [AIFunctionFactory.Create(ResearchSpaceFactsAsync), AIFunctionFactory.Create(GenerateCharacterProfilesAsync)]);


AgentRunOptions options = new()
{
    AllowBackgroundResponses = true
};

AgentThread thread = agent.GetNewThread();

AgentRunResponse response = await agent.RunAsync("写一部篇幅非常长的小说，内容是关于一支宇航员团队探索一片未知星系的故事。", thread, options);

// 循环直到后台完成
while (response.ContinuationToken is not null)
{
    PersistAgentState(thread, response.ContinuationToken);

    await Task.Delay(TimeSpan.FromSeconds(10));

#pragma warning disable MEAI001 
    RestoreAgentState(agent, out thread, out ResponseContinuationToken? continuationToken);
#pragma warning restore MEAI001 

    options.ContinuationToken = continuationToken;
    response = await agent.RunAsync(thread, options);
}

Console.WriteLine(response.Text);

#pragma warning disable MEAI001 
void PersistAgentState(AgentThread thread, ResponseContinuationToken? continuationToken)
{
    stateStore["thread"] = thread.Serialize();
    stateStore["continuationToken"] = JsonSerializer.SerializeToElement(continuationToken, AgentAbstractionsJsonUtilities.DefaultOptions.GetTypeInfo(typeof(ResponseContinuationToken)));
}
#pragma warning restore MEAI001

#pragma warning disable MEAI001
void RestoreAgentState(AIAgent agent, out AgentThread thread, out ResponseContinuationToken? continuationToken)
{
    JsonElement serializedThread = stateStore["thread"] ?? throw new InvalidOperationException("No serialized thread found in state store.");
    JsonElement? serializedToken = stateStore["continuationToken"];

    thread = agent.DeserializeThread(serializedThread);
    continuationToken = (ResponseContinuationToken?)serializedToken?.Deserialize(AgentAbstractionsJsonUtilities.DefaultOptions.GetTypeInfo(typeof(ResponseContinuationToken)));
}
#pragma warning restore MEAI001 
[Description("写一部非常长篇的小说，讲述一支宇航员团队探索一片尚未被发现的星系的故事。")]
async Task<string> ResearchSpaceFactsAsync(string topic)
{
    Console.WriteLine($"正在研究主题： {topic}");

    await Task.Delay(TimeSpan.FromSeconds(10));

    string result = topic.ToUpperInvariant() switch
    {
        var t when t.Contains("星系") => "研究结果：星系包含数十亿颗恒星。未被探索的星系可能具有独特的恒星形成、奇异物质和未被探索的现象，如暗能量浓度。",
        var t when t.Contains("太空") || t.Contains("TRAVEL") => "研究结果：星际旅行需要先进的推进系统。挑战包括辐射暴露、生命支持和在未知空间中的导航。",
        var t when t.Contains("宇航员") => "研究结果：宇航员在零重力环境、紧急协议、航天器系统和长期任务的团队动态方面接受严格培训。",
        _ => $"研究结果：与{topic}相关的一般太空探索事实。深空任务需要先进的技术、船员的韧性和对未知情况的应急计划。"
    };

    Console.WriteLine(" 研究完成");
    return result;
}

[Description("写一部非常长的小说，讲述一支宇航员团队探索一片尚未被发现的星系的故事。")]
async Task<IEnumerable<string>> GenerateCharacterProfilesAsync()
{
    Console.WriteLine("正在生成角色档案...");

    // Simulate a character generation operation
    await Task.Delay(TimeSpan.FromSeconds(10));

    string[] profiles = [
            @"伊莲娜·沃斯 上尉：一位经验丰富的任务指挥官，拥有 15 年的服役经验。性格坚毅、果断，但内心承受着对整个团队安危负责的沉重压力。前军用飞行员，后转为宇航员。",
            @"詹姆斯·陈 博士：首席科学官兼天体物理学家。才华横溢但社交笨拙，他在数据与探索中找到慰藉。强烈的好奇心常常将任务推向未知领域。",
            @"玛雅·托雷斯 中尉：导航专家，也是团队中最年轻的成员。乐观、精通技术，为各种挑战带来全新的视角和创新性的解决方案。",
            @"马库斯·里维拉 指挥官：首席工程师，精通飞船系统。务实、足智多谋，即使在资源极其有限的情况下也能修好几乎任何东西。把船员安全置于一切之上。",
            @"阿玛拉·奥卡福 博士：医疗官兼心理学家。富有同理心、观察力敏锐，在漫长的太空旅程中帮助维持团队士气和心理健康。空间医学专家。"
    ];

    Console.WriteLine($"生成了 {profiles.Length} 个角色档案");
    return profiles;
}


