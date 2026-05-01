using AspNetCore.FormatResponseOutputData.Models;
using Microsoft.AspNetCore.Mvc.Formatters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


//builder.Services.AddControllers()
//                .AddXmlSerializerFormatters();
//builder.Services.AddControllers()
//                .AddJsonOptions(options =>
//                {
//                    //JSON 序列化器将使用 .NET 对象的属性名作为 JSON 属性名，而不会进行任何额外的命名转换
//                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
//                });

//builder.Services.AddControllers()
//    .AddNewtonsoftJson();



builder.Services.AddSingleton<TodoItemStore>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
