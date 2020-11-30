using Microsoft.EntityFrameworkCore;
using System;
using GTDApp.Data;
using GTDApp.Models;

namespace Tests
{
    public class DbTest
    {
        public DatabaseContext MakeInMemoryContext()
        {
            return MakeDbContext(false);
        }

        public DatabaseContext MakeSqliteContext()
        {
            return MakeDbContext(true);
        }

        protected DatabaseContext MakeDbContext(bool useSqlite)
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>()
                              .EnableSensitiveDataLogging();

            builder = useSqlite
                ? builder.UseSqlite("Data Source=:memory:")
                : builder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            var context = new DatabaseContext(builder.Options);

            if (useSqlite)
            {
                context.Database.OpenConnection();
                context.Database.EnsureCreated();
            }

            SeedDatabase(context);

            context.ChangeTracker.Clear();

            return context;
        }

        protected virtual void SeedDatabase(DatabaseContext context) { }

    }
}
