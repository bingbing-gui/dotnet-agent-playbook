using AspNetCore.API.JWT.Authentication.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//配置Swagger
builder.Services.AddSwaggerGen(options =>
{
    //options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    //{
    //    Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
    //    In = ParameterLocation.Header,
    //    Name = "Authorization",
    //    Type = SecuritySchemeType.ApiKey
    //});
    //options.OperationFilter<SecurityRequirementsOperationFilter>();
    var schemeName = "bearer";
    //如果用Token验证，会在Swagger界面上有验证              
    options.AddSecurityDefinition(schemeName, new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "请输入不带有bearer的Token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = schemeName,
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = schemeName
                            }
                        },
                        new string[0]
                    }
                    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var secret = builder.Configuration.GetSection("JwtSetting:Secret").Value;
var issuer = builder.Configuration.GetSection("JwtSetting:Issuer").Value;
var audience = builder.Configuration.GetSection("JwtSetting:Audience").Value;
//对称加密
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
//签署凭证
var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512Signature);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        RequireExpirationTime = true
    };
});

builder.Services.AddSingleton<User>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
