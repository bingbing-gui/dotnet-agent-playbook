


using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

string connectionString = "";
string queueName = "notification-events";

var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender(queueName);

// 模拟一个业务事件
var messageBody = new
{
    Event = "PostLiked",
    UserId = 123,
    TargetUserId = 456,
    Time = DateTime.UtcNow
};

var message = new ServiceBusMessage(JsonSerializer.Serialize(messageBody))
{
    ContentType = "application/json"
};

await sender.SendMessageAsync(message);

Console.WriteLine("✅ 消息发送成功");

await sender.DisposeAsync();
await client.DisposeAsync();