var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var lifetime = app.Services.GetService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() => Console.WriteLine("===== Server is starting"));
lifetime.ApplicationStopping.Register(() =>Console.WriteLine("===== Server is stopping"));
lifetime.ApplicationStopped.Register(() => Console.WriteLine("===== Server has stopped"));

app.Run(async context =>
{
    await context.Response.WriteAsync("Hello world");
});

app.Run();

