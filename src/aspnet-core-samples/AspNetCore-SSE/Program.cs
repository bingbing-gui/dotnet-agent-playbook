using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/sse", (HttpContext context, CancellationToken cancellationToken) =>
{
    var lastEventIdHeader = context.Request.Headers["Last-Event-ID"].ToString();
    long.TryParse(lastEventIdHeader, out var lastEventId);

    var startEventId = lastEventId + 1;
    var connectionId = Guid.NewGuid().ToString("N")[..8];

    Console.WriteLine(
        "[{0:HH:mm:ss}] SSE connected. ConnectionId={1}, Last-Event-ID={2}",
        DateTimeOffset.Now,
        connectionId,
        lastEventId);

    return TypedResults.ServerSentEvents(
        StreamEvents(startEventId, connectionId, cancellationToken));
});

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    time = DateTimeOffset.Now
}));

app.Run();

static async IAsyncEnumerable<SseItem<SseMessage>> StreamEvents(
    long startEventId,
    string connectionId,
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    const int disconnectAfter = 10;

    try
    {
        for (var i = 0; i < disconnectAfter && !cancellationToken.IsCancellationRequested; i++)
        {
            var eventId = startEventId + i;

            var message = new SseMessage(
                Id: eventId,
                Message: $"这是服务器推送的第 {eventId} 条消息",
                ServerTime: DateTimeOffset.Now,
                ConnectionId: connectionId);

            yield return new SseItem<SseMessage>(message, "server-message")
            {
                EventId = eventId.ToString(),
                ReconnectionInterval = TimeSpan.FromSeconds(2)
            };
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
        Console.WriteLine(
            "[{0:HH:mm:ss}] Demo disconnect after {1} messages. ConnectionId={2}",
            DateTimeOffset.Now,
            disconnectAfter,
            connectionId);
    }
    finally
    {
        Console.WriteLine(
            "[{0:HH:mm:ss}] SSE disconnected. ConnectionId={1}",
            DateTimeOffset.Now,
            connectionId);
    }
}

public sealed record SseMessage(
    long Id,
    string Message,
    DateTimeOffset ServerTime,
    string ConnectionId);
