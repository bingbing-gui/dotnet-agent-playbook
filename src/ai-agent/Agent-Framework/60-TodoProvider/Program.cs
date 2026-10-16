// Copyright (c) Microsoft. All rights reserved.

// Todo List — Track work items across turns with TodoProvider
//
// This sample shows how to use the TodoProvider, an AIContextProvider that gives an agent a set of
// tools for managing a todo list (todos_add, todos_complete, todos_remove, todos_get_remaining,
// todos_get_all) along with instructions on how to use them. The todo list is stored in the
// session state and persists across turns, so the agent can plan multi-step work, track progress,
// and adjust the list as the conversation evolves.
//
// This is a scripted, non-interactive walkthrough: it sends a sequence of messages to the agent
// and, after each turn, prints the agent's reply followed by the current todo list (read directly
// from the provider via GetAllTodosAsync). This lets you watch the todo state evolve as the agent
// adds, completes, and removes items.

using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ClientModel.Primitives;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";
var imageDeployment = "gpt-image-2";//Environment.GetEnvironmentVariable("FOUNDRY_IMAGE_GENERATION_DEPLOYMENT") ?? throw new InvalidOperationException("FOUNDRY_IMAGE_GENERATION_DEPLOYMENT is not set.");


//#pragma warning disable MAAI001 
//using var todoProvider = new TodoProvider();
//#pragma warning restore MAAI001

//AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
//    .AsAIAgent(new ChatClientAgentOptions
//    {
//        Name = "PlanningAssistant",
//        ChatOptions = new ChatOptions
//        {
//            ModelId = model,
//            Instructions = "你是一个有帮助的计划助理。使用你的待办事项列表来计划和跟踪多步骤的工作。",
//        },
//        AIContextProviders = [todoProvider],
//    });

//AgentSession session = await agent.CreateSessionAsync();


//string[] userMessages =
//[
//    "我是组织一个小型团队外出活动。你能帮我计划吗？把工作分解成一个待办事项列表。",
//    "我已经预订了场地并发送了邀请。请更新列表。",
//    "实际上，我们跳过餐饮，改为计划一次集体徒步旅行。相应地更新计划。",
//];

//foreach (string userMessage in userMessages)
//{
//    Console.WriteLine($"用户: {userMessage}");
//    Console.WriteLine($"代理: {await agent.RunAsync(userMessage, session)}");

//    // Read the current todo list straight from the provider and print it so the state is visible.
//    await PrintTodoListAsync(todoProvider, session);
//    Console.WriteLine();
//}
//#pragma warning disable MAAI001 
//static async Task PrintTodoListAsync(TodoProvider todoProvider, AgentSession session)
//{
//    IReadOnlyList<TodoItem> todos = await todoProvider.GetAllTodosAsync(session);

//    Console.WriteLine("--- 当前待办事项列表 ---");
//    if (todos.Count == 0)
//    {
//        Console.WriteLine("  (空)");
//        return;
//    }

//    foreach (TodoItem todo in todos)
//    {
//        string status = todo.IsComplete ? "x" : " ";
//        Console.WriteLine($"  [{status}] {todo.Id}. {todo.Title}");
//    }
//}
//#pragma warning restore MAAI001



using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add(
    "x-ms-oai-image-generation-deployment",
    imageDeployment);

var clientOptions = new AIProjectClientOptions
{
    Transport = new HttpClientPipelineTransport(httpClient)
};

AIAgent agent = new AIProjectClient(
        new Uri(endpoint),
        new DefaultAzureCredential(),
        clientOptions)
   .AsAIAgent(model: model,"你是一个助手",
    tools:
    [
        new HostedImageGenerationTool
        {
            Options = new ImageGenerationOptions
            {
                ModelId = "gpt-image-2",
                MediaType = "image/png"
            }
        }
    ]);


AgentResponse response = await agent.RunAsync(
    "生成一张在太空中编写 C# 代码的机器人图片。");

foreach (ChatMessage message in response.Messages)
{
    foreach (ImageGenerationToolResultContent imageResult
        in message.Contents.OfType<ImageGenerationToolResultContent>())
    {
        foreach (DataContent image
            in imageResult.Outputs?.OfType<DataContent>() ?? [])
        {
            string filePath = Path.Combine(
                Environment.CurrentDirectory,
                $"generated-{Guid.NewGuid():N}.png");

            await File.WriteAllBytesAsync(
                filePath,
                image.Data.ToArray());

            Console.WriteLine($"图片已保存：{filePath}");
        }
    }
}
