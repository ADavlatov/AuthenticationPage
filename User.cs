using Microsoft.EntityFrameworkCore;

namespace AuthenticationPage;
public class User
{
    public User(string login, string password)
    {
        Login = login;
        Password = password;
    }

    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
}