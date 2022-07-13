namespace AuthenticationPage;
public class User
{
    public User(string login, string password)
    { 
        Login = login;
        Password = password;
    }

    public string Id { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
}