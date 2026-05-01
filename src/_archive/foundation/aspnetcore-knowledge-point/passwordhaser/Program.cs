using Microsoft.AspNetCore.Identity;

var app = WebApplication.Create();

app.Run(context =>
{
    var password = context.Request.Query["password"];

    if (string.IsNullOrWhiteSpace(password))
        password = "123456";

    var usr = new User();
    var hasher = new PasswordHasher<User>();
    var hashedPassword = hasher.HashPassword(usr, password);

    var isPasswordMatch = hasher.VerifyHashedPassword(usr, hashedPassword, password);

    return context.Response.WriteAsync($"Password : {password} => Hashed : {hashedPassword} \nPassword Matched : {isPasswordMatch}");
});

app.Run();

public record User();



