using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SoundPool.Data.EFCore
{
    public class BaseDesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T>
        where T : DbContext
    {
        public T CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<LibraryContext>();

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("dbsettings.json", optional: false, reloadOnChange: true)
                .Build();

            builder.UseSqlite(configBuilder.GetConnectionString(typeof(T).Name));

            return Activator.CreateInstance(
                typeof(T), builder.Options) as T;
        }
    }
}