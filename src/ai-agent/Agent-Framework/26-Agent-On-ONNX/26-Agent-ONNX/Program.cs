
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntimeGenAI;


using OnnxRuntimeGenAIChatClient chatClient = new(@"E:\Model Catalog\Phi-4-mini-instruct-onnx\cpu_and_mobile\cpu-int4-rtn-block-32-acc-level-4");
AIAgent agent = chatClient.AsAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");

// Invoke the agent and output the text result.
Console.WriteLine(await agent.RunAsync("给我讲一个发生在茶馆里的段子。"));