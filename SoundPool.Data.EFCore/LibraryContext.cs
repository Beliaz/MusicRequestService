using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace SoundPool.Data.EFCore
{
    public class LibraryContext : DbContext
    {
        protected LibraryContext()
        {
        }

        public LibraryContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; }

        public DbSet<Artist> Artists { get; set; }
    }
}