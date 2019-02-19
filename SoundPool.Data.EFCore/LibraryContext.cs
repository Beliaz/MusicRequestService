using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace SoundPool.Data.EFCore
{
    public class LibraryContext : DbContext
    {
        protected LibraryContext()
        {
        }

        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; }

        public DbSet<Artist> Artists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Song>(b =>
            {
                b.HasKey(s => s.Id);
                b.HasMany(s => s.Artists)
                    .WithOne(a => a.Song)
                    .HasForeignKey(s => s.SongId);
            });

            modelBuilder.Entity<Artist>(b =>
            {
                b.HasKey(a => a.Name);
                b.HasMany(a => a.Songs)
                    .WithOne(s => s.Artist)
                    .HasForeignKey(s => s.ArtistId);
            });

            modelBuilder.Entity<SongUrl>().HasKey(u => u.Url);

            modelBuilder.Entity<SongArtist>(b =>
            {
                b.HasKey(a => a.Id);

                b.HasOne(a => a.Song)
                    .WithMany(a => a.Artists);

                b.HasOne(a => a.Artist)
                    .WithMany(a => a.Songs);
            });
        }
    }
}