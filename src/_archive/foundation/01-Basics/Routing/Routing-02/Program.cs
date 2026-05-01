using AspNetCore.RouteConstraint.CustomConstraint;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<RouteOptions>(options =>
   options.ConstraintMap.Add("allowedgods", typeof(OnlyGodsConstraint)));

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

#region Int约束
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id:int}");
#endregion

#region 范围约束
//app.MapControllerRoute(
//    name: "rangeConstraint",
//    pattern: "{controller=Home}/{action=Index}/{id:range(5,20)?}");
#endregion

#region 路由正则约束
//app.MapControllerRoute(
//    name: "regexConstraint",
//    pattern: "{controller:regex(^H.*)=Home}/{action=Index}/{id?}");

//app.MapControllerRoute(
//    name: "regexConstraint",
//    pattern: "{controller:regex(^H.*)=Home}/{action:regex(^Index$|^About$)=Index}/{id?}");
#endregion

#region 组合路由约束
//app.MapControllerRoute(
//    name: "combiningConstraint",
//    pattern: "{controller=Home}/{action=Index}/{id:alpha:regex(^H.*)?}");
#endregion

#region 客户自定义约束
app.MapControllerRoute(
    name: "customerConstraint",
    pattern: "{controller=Home}/{action=Index}/{id:allowedgods}"
    );
#endregion

app.Run();
