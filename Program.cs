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

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.Map("/info", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/user_info.html");
});
app.MapGet("/isauth", async (context) =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        string isAuth = context.Request.Cookies["isAuth"];
        await context.Response.WriteAsJsonAsync(isAuth);
    }
});
app.MapGet("/log-in", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/login_form.html");
});
/*app.MapGet("/sign-in", async (context) =>aa
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/signin_form.html");
});*/
app.MapPost("/log-in", async (string? returnUrl, HttpContext context) =>
{
    var form = context.Request.Form;
    if (!form.ContainsKey("login") || !form.ContainsKey("password"))
        return Results.BadRequest("Логин или пароль не установлены");

    string username = form["login"];
    string login = form["login"];
    string password = form["password"];

    context.Response.Cookies.Append("username", username);
    context.Response.Cookies.Append("isAuth", "auth");

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
        context.Response.Cookies.Delete("username");
        return Results.Redirect(returnUrl??"/");
    }
    else
    {
        return Results.Redirect(returnUrl??"/");
    }
});

app.MapGet("/info", async (context) =>
{
    string name = context.Request.Cookies["username"];
    await context.Response.WriteAsJsonAsync(name);
});

app.Run();