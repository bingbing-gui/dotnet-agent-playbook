using Microsoft.SemanticKernel;
using SK.TextEmbeddingGeneration.Models;
using SK.TextEmbeddingGeneration.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddPostgresVectorStore(
    builder.Configuration.GetConnectionString("Postgres")!
);

builder.Services.Configure<AzureEmbeddingOptions>(
    builder.Configuration.GetSection("Embedding"));

//builder.Services.AddSingleton(sp =>
//{
//    var endpoint = builder.Configuration["Embedding:Endpoint"]
//        ?? throw new InvalidOperationException("Missing AzureOpenAI:Endpoint");
//    var key = builder.Configuration["Embedding:ApiKey"]
//        ?? throw new InvalidOperationException("Missing AzureOpenAI:ApiKey");
//    return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
//});
# region SK Embedding服务

builder.Services.AddSingleton<IEmbeddingService, AzureOpenAIEmbeddingService>();

builder.Services.AddScoped<IHotelService, HotelService>();

#pragma warning disable SKEXP0010
builder.Services.AddAzureOpenAIEmbeddingGenerator(
    deploymentName: builder.Configuration["Embedding:DeploymentName"], // 模型部署名字, 例如. "text-embedding-ada-002".
    endpoint: builder.Configuration["Embedding:Endpoint"],           // Azure Open AI终结点 例如. https://myaiservice.openai.azure.com.
    apiKey: builder.Configuration["Embedding:ApiKey"],
    //modelId: "MODEL_ID",          // 如果部署名和模型名一致，可以不填。如果你在 Azure OpenAI，你传入的是 部署名（deploymentName），它可能和实际模型名不一样。比如你在 Azure Portal 部署时叫 "embeddings", 但底层模型其实是 "text-embedding-ada-002"，这时你就可以用 modelId 来告诉 SK 使用哪个模型
    //serviceId: "YOUR_SERVICE_ID", // 给这个服务注册一个 唯一标识，方便在应用里区分多个服务。
    dimensions: 1536              // O指定向量的维度大小
);
builder.Services.AddSingleton((serviceProvider) =>
{
    return new Kernel(serviceProvider);
});
#endregion



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
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
