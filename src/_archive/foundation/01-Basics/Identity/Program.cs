using Identity.CommonService;
using Identity.CustomPolicy;
using Identity.IdentityPolicy;
using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<AppUser, Role>().
AddEntityFrameworkStores<AppIdentityDbContext>().
AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromHours(10));

builder.Services.AddDbContext<AppIdentityDbContext>(
    options => options.UseSqlite(builder.Configuration["ConnectionStrings:DefaultConnection"])
    );

builder.Services.Configure<IdentityOptions>(options =>
{

    options.User.RequireUniqueEmail = true;

    // 允许任何字符集输入
    options.User.AllowedUserNameCharacters = null; 
    //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";

    #region 设置密码策略
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = true;
    #endregion
    #region 启用Email确认
    options.SignIn.RequireConfirmedEmail = true;
    #endregion

    #region 设置用户登陆次数
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(10);
    options.Lockout.MaxFailedAccessAttempts = 3;
    #endregion

});

builder.Services.ConfigureApplicationCookie(
        opts =>
        {
            //默认登录页面
            opts.LoginPath = "/Account/Login";
            opts.AccessDeniedPath = "/Account/AccessDenied";
            //设置Cookie名称
            opts.Cookie.Name = ".AspNetCore.Identity.Application";
            //设置Cookie超时时间
            opts.ExpireTimeSpan = TimeSpan.FromMinutes(20);
            //设置滑动时间
            opts.SlidingExpiration = true;
        }
    );

builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSetting"));

#region 授权策略

builder.Services.AddTransient<IAuthorizationHandler, AllowUsersHandler>();
builder.Services.AddTransient<IAuthorizationHandler, AllowPrivateHandler>();
builder.Services.AddAuthorization(authorizationOptions =>
{
    authorizationOptions.AddPolicy("AspManager", authorizationPolicy =>
    {
        authorizationPolicy.RequireRole("Manager");
        authorizationPolicy.RequireClaim("Coding-Skill", "ASP.NET Core MVC");
    });
    authorizationOptions.AddPolicy("AllowTom", authorizationPolicy =>
    {
        authorizationPolicy.AddRequirements(new AllowUserPolicy("tom"));
    });
    authorizationOptions.AddPolicy("PrivateAccess", authorizationPolicy =>
    {
        authorizationPolicy.AddRequirements(new AllowPrivatePolicy());
    });
});
#endregion

// Add services to the container.
builder.Services.AddControllersWithViews();
#region 自定义密码、用户名、邮箱策略
//builder.Services.AddTransient<IPasswordValidator<AppUser>, CustomPasswordPolicy>();
//builder.Services.AddTransient<IUserValidator<AppUser>, CustomUsernameEmailPolicy>();
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
