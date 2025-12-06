using Microsoft.EntityFrameworkCore;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Data 
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Customers { get; set; }
        public DbSet<Game> Games { get; set; }
    }
}
 