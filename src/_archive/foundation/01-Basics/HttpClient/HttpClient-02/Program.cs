using AspNetCore.HttpClientWithHttpVerb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoContext>(options =>
              options.UseInMemoryDatabase("TodoItems"));

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<TodoClient>((serviceProvider, httpClient) =>
{
    var httpRequest = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.Request;
    httpClient.BaseAddress = new Uri(UriHelper.BuildAbsolute(
        httpRequest.Scheme, httpRequest.Host, httpRequest.PathBase));
    httpClient.Timeout = TimeSpan.FromSeconds(5);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, policyErrors) =>
    {
        return true;
    }
}
);
//builder.Services.AddScoped<IOperationScoped, OperationScoped>();

//builder.Services.AddTransient<OperationHandler>();
//builder.Services.AddTransient<OperationResponseHandler>();

//builder.Services.AddHttpClient("operation")
//    .AddHttpMessageHandler<OperationHandler>()
//    .AddHttpMessageHandler<OperationResponseHandler>()
//    .SetHandlerLifetime(TimeSpan.FromSeconds(50));

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
