
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async context =>
{
    var connection = context.Connection;

    var connectionInfo = new
    {
        LocalIpAddress = connection.LocalIpAddress?.ToString(),
        LocalPort = connection.LocalPort,
        RemoteIpAddress = connection.RemoteIpAddress?.ToString(),
        RemotePort = connection.RemotePort,
        ClientCertificate = connection.ClientCertificate?.FriendlyName,
        ConnectionId = connection.Id
    };

    var json = System.Text.Json.JsonSerializer.Serialize(connectionInfo);
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(json);
});

app.Run();
