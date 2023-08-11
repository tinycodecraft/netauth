using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWithAuth;

public class UsersContext : IdentityUserContext<IdentityUser>
{
    protected readonly IConfiguration  itconfig;
    public UsersContext (DbContextOptions<UsersContext> options,IConfiguration configuration)
        : base(options)
    {
        
        itconfig = configuration;
    }
        
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        
        // It would be a good idea to move the connection string to user secrets
        var conn = itconfig.GetConnectionString("userDatabase") ?? "Host=localhost;Database=userdata;Username=reactuser;Password=P@ssw0rd";
        //the following is to indicate the db is postgresql
        options.UseNpgsql(conn);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}