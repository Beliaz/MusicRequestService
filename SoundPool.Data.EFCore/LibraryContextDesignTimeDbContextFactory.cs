using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SoundPool.Data.EFCore
{
    public class LibraryContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<LibraryContext>
    {
        public LibraryContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<LibraryContext>();

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("dbsettings.json", optional: false, reloadOnChange: true)
                .Build();

            builder.UseSqlite(configBuilder["ConnectionString"]);

            return new LibraryContext(builder.Options);
        }
    }
}