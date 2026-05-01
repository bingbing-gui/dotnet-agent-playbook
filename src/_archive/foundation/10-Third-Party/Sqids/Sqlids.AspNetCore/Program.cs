var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 注册 Sqids 服务
builder.Services.AddSingleton(new Sqids.SqidsEncoder<int>());

// 注册控制器服务
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapControllers();

app.Run();