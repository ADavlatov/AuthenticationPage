using System.Security.Claims;
using AuthenticationPage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

var users = new List<User>
{
    new User("Qwerty11", "qwerty11@example.com", "123456"),
    new User("Qwerty12", "qwerty12@example.com", "12345678")
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Cookies").AddCookie(options => options.LoginPath = "/");
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/",  async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});
app.MapGet("/postisauth", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";

    if (context.User.Identity.IsAuthenticated)
    {
        await context.Response.WriteAsync("Hello");
    }
    else if (!context.User.Identity.IsAuthenticated)
    {
        await context.Response.WriteAsync("Error 401");
    }
});
app.MapGet("/login", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/login_form.html");
});
app.MapGet("/signin", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/signin_form.html");
});
app.MapPost("login", async (string? returnUrl, HttpContext context) =>
{
    var form = context.Request.Form;
    if (!form.ContainsKey("login") || !form.ContainsKey("password"))
        return Results.BadRequest("Логин или пароль не установлены");

    string login = form["login"];
    string password = form["password"];

    User? user = users.FirstOrDefault(user => (user.Login == login || user.Email == login) && user.Password == password);
    if (user == null)
        return Results.Unauthorized();

    var claims = new List<Claim> {new Claim(ClaimTypes.Name, user.Login, user.Password)};
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    await context.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));
    return Results.Redirect(returnUrl??"/");
});

    app.Run();