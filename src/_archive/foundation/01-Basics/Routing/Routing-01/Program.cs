var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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


#region 默认路由

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

#endregion

#region 不指定默认路由

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller}/{action}");

#endregion
#region 静态路由
app.MapControllerRoute(
    name: "news1",
    pattern: "News/{controller=Home}/{action=Index}");

//app.MapControllerRoute(
//    name: "news2",
//    pattern: "News{controller}/{action}");

#endregion



#region 保留老的路由(https://localhost:7134/Shopping/Index 路由到https://localhost:7134/Home/Index )


//app.MapControllerRoute(
//    name: "shop",
//    pattern: "Shopping/{action}",
//    defaults: new { controller = "Home" });

#endregion

#region Controller和Action在路由中的默认值 (https://localhost:7134/Shopping/Old 路由到Home的Index 方法)

//app.MapControllerRoute(
//    name: "old",
//    pattern: "Shopping/Old",
//    defaults: new { controller = "Home", action = "Index" });

#endregion

#region ASP.NET Core 多个路由 ( 路由的顺序不同决定了调用不同的方法 ) 
//app.MapControllerRoute(
//    name: "old",
//    pattern: "Shopping/Old",
//    defaults: new { controller = "Home", action = "Index" });
//​
//app.MapControllerRoute(
//    name: "shop",
//    pattern: "Shopping/{action}",
//    defaults: new { controller = "Home" });
#endregion

#region 客户自定义段

//app.MapControllerRoute(
//    name: "MyRoute",
//    pattern: "{controller=Home}/{action=Index}/{id}");

#endregion

#region 可选参数类型

//app.MapControllerRoute(
//    name: "MyRoute1",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

#endregion

#region 路由通配符的*catchall

//app.MapControllerRoute(
//    name: "MyRoute2",
//    pattern: "{controller=Home}/{action=Index}/{id?}/{*catchall}");

#endregion

app.Run();

