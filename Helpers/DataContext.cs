namespace WebApi.Helpers;

using Microsoft.EntityFrameworkCore;
using WebApi.Entities;

/// <summary>
/// Database Context used by EF Core to interface
/// with SQL Server.
/// </summary>
public class DataContext : DbContext
{
    /// <summary>
    /// App Configuration
    /// </summary>
    protected readonly IConfiguration Configuration;

    /// <summary>
    /// Class constructor. 
    /// </summary>
    /// <param name="configuration"></param>
    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Override method. Used to specify connection settings to
    /// connect to DB
    /// </summary>
    /// <param name="options"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sql server database
        options.UseSqlServer(Configuration.GetConnectionString("WebApiDatabase"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Make the 'Email' field on the Users table an index and 
        // ensure that it is unique - no duplicate email entries.
        modelBuilder.Entity<User>()
            .HasIndex(i => i.Email)
            .IsUnique();
    }

    /// <summary>
    /// POC representing the Users table
    /// </summary>
    public virtual DbSet<User> Users { get; set; }
}