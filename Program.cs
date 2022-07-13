using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthenticationPage;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

DefaultFilesOptions options = new DefaultFilesOptions();
options.DefaultFileNames.Add("wwwroot");

builder.Services.AddAuthentication("Cookies").AddCookie(options => options.LoginPath = "/");
builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationContext>();

var app = builder.Build();

app.UseDefaultFiles(options);
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.Map("/userinfo", async (context) =>
{
    string? name = context.Request.Cookies["username"];
    await context.Response.WriteAsJsonAsync(name);
});
app.Map("/authinfo", async (context) =>
{
    if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
    {
        string? isAuth = context.Request.Cookies["isAuth"];
        await context.Response.WriteAsJsonAsync(isAuth);
    }
});
app.Map("/log-in", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/login_form.html");
});
app.Map("/sign-in", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/signin_form.html");
});
app.Map("/info", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/user_info.html");
});
app.Map("/change-password", async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/change_password.html");
});
app.Map("/log-out", async (string? returnUrl, HttpContext context) =>
{
    context.Response.Cookies.Delete("username");
    context.Response.Cookies.Delete("isAuth");
    if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
    {
        await context.SignOutAsync("Cookies");
        return Results.Redirect(returnUrl??"/");
    }
    else
    {
        return Results.Redirect(returnUrl??"/");
    }
});
app.MapPost("/log-in", async (string? returnUrl, ApplicationContext db, HttpContext context) =>
{
    var form = context.Request.Form;
    if (!form.ContainsKey("login") || !form.ContainsKey("password"))
        return Results.BadRequest("Логин или пароль не установлены");

    string login = form["login"]!;
    string password = form["password"]!;

    password = GetHash(password);

    User? user = db.Users.FirstOrDefault(user => user.Login == login & user.Password == password);
    if (user == null)
        return Results.BadRequest("Вы ввели неверный логин или пароль");

    var claims = new List<Claim> {new Claim(ClaimTypes.Name, user.Login, user.Password)};
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    await context.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));
    
    if (context.User.Identity != null && !(context.User.Identity.IsAuthenticated))
    {
        context.Response.Cookies.Append("username", login);
        context.Response.Cookies.Append("isAuth", "auth");  
    }
    
    return Results.Redirect(returnUrl??"/");
});
app.MapPost("/sign-in", (string? returnUrl, ApplicationContext db, HttpContext context) =>
{
    var form = context.Request.Form;
    
    string login = form["login"]!;
    string password = form["password"]!;

    foreach (var user in db.Users)
    {
        if (user.Login == login)
        {
            return Results.BadRequest("Такой пользователь уже существует");
        }
    }
    
    db.Users.Add(new User{Login = login, Password = GetHash(password)});
    db.SaveChanges();

    return Results.Redirect(returnUrl??"/");
});
app.MapPost("/change", (string? returnUrl, ApplicationContext db, HttpContext context) =>
{
    var form = context.Request.Form;

    string login = context.Request.Cookies["username"]!;
    string oldPassword = form["oldPassword"]!;
    string newPassword = form["newPassword"]!;

    oldPassword = GetHash(oldPassword);

    User? user = db.Users.FirstOrDefault(user => user.Login == login && user.Password == oldPassword);
    if (user == null)
        return Results.BadRequest("Неверный пароль");

    user.Password = GetHash(newPassword);
    db.SaveChanges();
    
    return Results.Redirect(returnUrl??"/");
});

app.Run();

string GetHash(string password)
{
    using (var hashAlg = MD5.Create())
    {
        byte[] hash = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(password));
        var builder = new StringBuilder(hash.Length*2);
        for (int i = 0; i < hash.Length; i++)
        {
            builder.Append(hash[i].ToString("X2"));
        }
        return builder.ToString();
    }   
}