using AspNetCore.OptionsPattern.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<PositionOptions>(
    builder.Configuration.GetSection(PositionOptions.Position));

//builder.Services.AddOptions<MyConfigOptions>()
//            .Bind(builder.Configuration.GetSection(MyConfigOptions.MyConfig))
//            .ValidateDataAnnotations()
//            .Validate(config =>
//            {
//                if (config.Key2 != 0)
//                {
//                    return config.Key3 > config.Key2;
//                }

//                return true;
//            }, "Key3 must be > than Key2.");   // Failure message.;

builder.Services.AddOptions<MyConfigOptions>()
           .Bind(builder.Configuration.GetSection(MyConfigOptions.MyConfig));

builder.Services.AddSingleton<IValidateOptions
                              <MyConfigOptions>, MyConfigValidation>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
