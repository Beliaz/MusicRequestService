using System.Linq;

namespace SoundPool.Data.EFCore
{
    public static class DbInitializer
    {
        public static void Initialize(LibraryContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Songs.Any())
            {
                return; // DB has been seeded
            }

            var artists = new[]
            {
                new Artist {Name = "KSHMR"},
                new Artist {Name = "Matisse & Sadko"},
                new Artist {Name = "Tritonal"},
            };

            foreach (var artist in artists)
            {
                context.Artists.Add(artist);
            }

            context.SaveChanges();

            var songs = new[]
            {
                new Song {Title = "Carry Me Home", Artist = context.Artists.First(a => a.Name.Equals("KSHMR"))},
                new Song {Title = "Grizzly", Artist = context.Artists.First(a => a.Name.Equals("Matisse & Sadko"))},
                new Song {Title = "Out My Mind", Artist = context.Artists.First(a => a.Name.Equals("Tritonal"))},
            };

            foreach (var s in songs)
            {
                context.Songs.Add(s);
            }

            context.SaveChanges();
        }
    }
}