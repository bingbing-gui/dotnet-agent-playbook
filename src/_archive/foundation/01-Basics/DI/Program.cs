using AspNetCore.DependencyInjection.Models;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddTransient<IRepository, Repository>();
//builder.Services.AddScoped<IRepository, Repository>();
//builder.Services.AddSingleton<IRepository,Repository>();
builder.Services.AddTransient<ProductSum>();
builder.Services.AddTransient<IRepository, Repository>();
builder.Services.AddTransient<IStorage, Storage>();
//IWebHostEnvironment env = builder.Environment;
//builder.Services.AddTransient<IRepository>(provider =>
//{
//    if (env.IsDevelopment())
//    {
//        var x = provider.GetService<Repository>();
//        return x;
//    }
//    else
//    {
//        return new ProductionRepository();
//    }

//});
builder.Host.ConfigureAppConfiguration((hostingContext, configuration) =>
{
    configuration.AddJsonFile("mysettings.json",
                    optional: false, //file is not optional
                    reloadOnChange: true);
}
);

builder.Services.Configure<MyJson>(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews();
var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    var storage = services.GetRequiredService<IStorage>();
}

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
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
