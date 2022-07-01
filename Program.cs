using System.Security.Claims;
using AuthenticationPage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

var users = new List<User>
{
    new User("Qwerty11",  "123456"),
    new User("Qwerty12", "12345678")
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Cookies").AddCookie(options => options.LoginPath = "/");
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

string username = null;

app.MapGet("/",  async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});
app.MapPost("/", () =>
{
    var response = new
    {
        name = username
    };
    return Results.Json(response);
});
app.MapGet("/isauth", async (context) =>
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
app.MapGet("/log-in", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/login_form.html");
});
/*app.MapGet("/sign-in", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/signin_form.html");
});*/
app.MapPost("/log-in", async (string? returnUrl, HttpContext context) =>
{
    var form = context.Request.Form;
    if (!form.ContainsKey("login") || !form.ContainsKey("password"))
        return Results.BadRequest("Логин или пароль не установлены");

    username = form["login"];
    string login = form["login"];
    string password = form["password"];

    User? user = users.FirstOrDefault(user => user.Login == login && user.Password == password);
    if (user == null)
        return Results.Unauthorized();

    var claims = new List<Claim> {new Claim(ClaimTypes.Name, user.Login, user.Password)};
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    await context.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));
    return Results.Redirect(returnUrl??"/");
});
app.MapPost("/log-out", async (string? returnUrl, HttpContext context) =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        await context.SignOutAsync("Cookies");
        return Results.Redirect(returnUrl??"/");
    }
    else
    {
        return Results.Redirect(returnUrl??"/");
    }
});

app.Run();