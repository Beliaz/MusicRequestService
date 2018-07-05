using Microsoft.EntityFrameworkCore;
using SoundPool.Data;

namespace SoundPool
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; }

        public DbSet<Artist> Artists { get; set; }
    }
}