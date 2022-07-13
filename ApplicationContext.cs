using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationPage;

public class ApplicationContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public ApplicationContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data SourceFilter=Authentication.db");
    }
}