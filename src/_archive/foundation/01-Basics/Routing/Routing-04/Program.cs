var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

#region 在网页尾部添加斜杠/  小写URL
builder.Services.Configure<RouteOptions>(options =>
{
    options.AppendTrailingSlash = true;
    options.LowercaseUrls = true;
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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

#region 
//app.MapControllerRoute(
//    name: "defaultonly",
//    pattern: "{controller}/{action}",
//    defaults: new { controller = "USA" });
#endregion
#region
app.MapControllerRoute(
    name: "stock",
    pattern: "Stock/{action}",
    defaults: new { controller = "Home" });
#endregion



app.MapControllerRoute(
    name: "sales",
    pattern: "sales/{controller=Home}/{action=Index}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
