using WebService.Domain;
using Microsoft.EntityFrameworkCore;

namespace WebService.Persistence;

public class AppDbContext(DbContextOptions options) : DbContext (options)
{
    public DbSet<Event> Events { get; set; }

}
