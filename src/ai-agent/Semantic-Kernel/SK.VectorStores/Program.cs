using Azure;
using Azure.AI.OpenAI;
using SK.VectorStores.Models;
using SK.VectorStores.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddPostgresVectorStore(
    builder.Configuration.GetConnectionString("Postgres")!
);

// 你的业务服务
builder.Services.AddScoped<HotelService>();


builder.Services.Configure<AzureEmbeddingOptions>(
    builder.Configuration.GetSection("Embedding"));

builder.Services.AddSingleton(sp =>
{
    var endpoint = builder.Configuration["Embedding:Endpoint"]
        ?? throw new InvalidOperationException("Missing AzureOpenAI:Endpoint");
    var key = builder.Configuration["Embedding:ApiKey"]
        ?? throw new InvalidOperationException("Missing AzureOpenAI:ApiKey");
    return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
});



builder.Services.AddSingleton<IEmbeddingService, AzureOpenAIEmbeddingService>();
builder.Services.AddScoped<IHotelService, HotelService>();

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
