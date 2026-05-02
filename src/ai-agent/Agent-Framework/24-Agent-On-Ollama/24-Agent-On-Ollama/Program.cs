
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = "http://localhost:11434/";
var modelName = "qwen3:1.7b";


AIAgent agent = new OllamaApiClient(new Uri(endpoint), modelName)
    .AsAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");

await foreach (var chunk in agent.RunStreamingAsync("给我讲一个发生在茶馆里的段子。"))
{
    Console.Write(chunk);
}
