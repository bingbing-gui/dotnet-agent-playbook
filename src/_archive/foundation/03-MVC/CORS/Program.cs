var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();

//builder.Services.AddCors();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy",
        builder => builder.WithOrigins("http://www.domain.com"));
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
app.UseStaticFiles();
app.UseRouting();
// Shows UseCors with CorsPolicyBuilder.
//app.UseCors(builder =>
//{
//    builder.WithOrigins(new string[] { "https://example1.com", "https://example2.com" })
//           .AllowAnyOrigin()
//           .AllowAnyMethod()
//           .AllowAnyHeader();
//});
app.UseCors("MyPolicy");
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
