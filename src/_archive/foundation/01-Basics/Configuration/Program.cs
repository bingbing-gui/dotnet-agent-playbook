using AspNetCore.Configuration.Middlewares;
using AspNetCore.Configuration.Models;
using AspNetCore.Configuration.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//builder.Services.AddRazorPages();
builder.Services.AddSingleton<TotalUsers>();

builder.Services.Configure<MyWebApi>(builder.Configuration.GetSection("APIEndpoints"));

builder.Services.Configure<Connections>(builder.Configuration.GetSection("ConnectionStrings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days.
    app.UseHsts();

}
else
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePages();
}  

app.UseHttpsRedirection();


//if (Convert.ToBoolean(app.Configuration["Middleware:EnableResponseEditingMiddleware"]))
//{
//    app.UseMiddleware<ResponseEditingMiddleware>();
//}​
//if (Convert.ToBoolean(app.Configuration["Middleware:EnableRequestEditingMiddleware"]))
//{
//    app.UseMiddleware<RequestEditingMiddleware>();
//}​
//if (Convert.ToBoolean(app.Configuration["Middleware:EnableShortCircuitMiddleware"]))
//{
//    app.UseMiddleware<ShortCircuitMiddleware>();
//}​
//if (Convert.ToBoolean(app.Configuration["Middleware:EnableContentMiddleware"]))
//{
//    app.UseMiddleware<ContentMiddleware>();
//}
if ((app.Configuration.GetSection("Middleware")?.GetValue<bool>("EnableResponseEditingMiddleware")).Value)
{
    app.UseMiddleware<ResponseEditingMiddleware>();
}
if ((app.Configuration.GetSection("Middleware")?.GetValue<bool>("EnableRequestEditingMiddleware")).Value)
{
    app.UseMiddleware<RequestEditingMiddleware>();
}
if ((app.Configuration.GetSection("Middleware")?.GetValue<bool>("EnableShortCircuitMiddleware")).Value)
{
    app.UseMiddleware<ShortCircuitMiddleware>();
}
if ((app.Configuration.GetSection("Middleware")?.GetValue<bool>("EnableContentMiddleware")).Value)
{
    app.UseMiddleware<ContentMiddleware>();
}

//app.UseDeveloperExceptionPage();
//app.UseStatusCodePages();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
