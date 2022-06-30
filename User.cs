namespace AuthenticationPage;

public class User
{
    public User(string login, string email, string password)
    {
        Login = login;
        Email = email;
        Password = password;
    }

    public string Login { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}