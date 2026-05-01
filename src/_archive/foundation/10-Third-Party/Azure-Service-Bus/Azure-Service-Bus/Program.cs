using Azure.Messaging.ServiceBus;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

string connectionString = "";
string queueName = "notification-events";

var client = new ServiceBusClient(connectionString);

var processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions
{
    AutoCompleteMessages = false
});

// 收到消息
processor.ProcessMessageAsync += async args =>
{
    var body = args.Message.Body.ToString();

    Console.WriteLine($"📩 收到消息: {body}");

    // 模拟处理
    await Task.Delay(500);

    Console.WriteLine("✅ 处理完成");

    // ACK
    await args.CompleteMessageAsync(args.Message);
};

// 错误处理
processor.ProcessErrorAsync += args =>
{
    Console.WriteLine($"❌ 错误: {args.Exception}");
    return Task.CompletedTask;
};

Console.WriteLine("🎧 开始监听...");
await processor.StartProcessingAsync();

Console.ReadLine();

await processor.StopProcessingAsync();
await processor.DisposeAsync();
await client.DisposeAsync();