using DocumentIntelligence.LocalDemo;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:5189");
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = PdfApi.MaxPdfBytes + 65536);
builder.Services.AddSingleton<IPdfAnalyzer, AzurePdfAnalyzer>();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapPdfApi();
app.Run();
