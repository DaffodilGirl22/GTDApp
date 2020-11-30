using Microsoft.EntityFrameworkCore;
using GTDApp.Models;

namespace GTDApp.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options) { }

        public DbSet<Inbox> Inbox { get; set; }

    }
}
