var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()  // 允许任何来源
                   .AllowAnyMethod()  // 允许任何请求方法
                   .AllowAnyHeader();  // 允许任何请求头
        });
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

// Place UseCors right after UseRouting and before UseAuthorization
app.UseRouting();

// Enable CORS for the entire app
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapStaticAssets();  // Serve static files

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}/{id?}");

// Run the app
app.Run();
