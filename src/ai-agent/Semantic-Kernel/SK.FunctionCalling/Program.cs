using Microsoft.SemanticKernel;
using SK.FunctionCalling.Plugins;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 注册 Semantic Kernel 服务（如果希望通过依赖注入方式使用）

builder.Services.AddSingleton(sp =>
{
    var kernel = Kernel.CreateBuilder()
     .AddAzureOpenAIChatCompletion(
         deploymentName: "gpt-4.1", // 你在 Azure 上配置的模型部署名称
         endpoint: "",
         apiKey: ""
     ).Build();
    var logger=kernel.LoggerFactory;
    kernel.ImportPluginFromType<TimePlugin>("Time");
    kernel.ImportPluginFromType<OrderBookPlugin>("OrderBook");
    return kernel;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Time}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
