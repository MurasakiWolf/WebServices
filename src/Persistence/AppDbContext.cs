using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class AppDbContext(DbContextOptions options) : DbContext (options)
{
    public DbSet<Activity> Activities { get; set; }

}
