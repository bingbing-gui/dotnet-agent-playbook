
using System.Text;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

// 创建Agent
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");

// 开始Agent会话
AgentThread thread = agent.GetNewThread();

// 运行Agent
Console.WriteLine(await agent.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。", thread));

// 序列化线程状态以便稍后恢复
JsonElement serializedThread = thread.Serialize();

// 保存序列化的线程到临时文件
string tempFilePath = Path.GetTempFileName();
await File.WriteAllTextAsync(tempFilePath, JsonSerializer.Serialize(serializedThread));

// 从临时文件加载序列化的线程（仅用于演示）。
JsonElement reloadedSerializedThread = JsonElement.Parse(await File.ReadAllTextAsync(tempFilePath));

// 反序列化线程以恢复状态
AgentThread resumedThread = agent.DeserializeThread(reloadedSerializedThread);

Console.WriteLine(await agent.RunAsync("现在把这个段子加上一些表情符号，并用说书人的语气再讲一遍。", resumedThread));