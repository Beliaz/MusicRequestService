using System.Collections.Generic;
using System.Linq;
using SoundPool.Data;
using SoundPool.Data.EFCore;
using Z.EntityFramework.Plus;

namespace SoundPool
{
    public static class DbInitializer
    {
        public static void Initialize(LibraryContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            //context.Artists.Delete();
            //context.Songs.Delete();
            //context.SaveChanges();

            if (context.Songs.Any())
            {
                return; // DB has been seeded
            }

            var artists = new[]
            {
                new Artist {Name = "KSHMR"},
                new Artist {Name = "Matisse & Sadko"},
                new Artist {Name = "Tritonal"},
                new Artist {Name = "Apek"},
            };

            foreach (var artist in artists)
            {
                context.Artists.Add(artist);
            }

            context.SaveChanges();

            var songs = new[]
            {
                new Song
                {
                    Title = "Carry Me Home",
                    Artists = context.Artists.Where(a => a.Name.Equals("KSHMR")).ToList()
                },
                new Song
                {
                    Title = "Grizzly",
                    Artists = context.Artists.Where(a => a.Name.Equals("Matisse & Sadko")).ToList()
                },
                new Song
                {
                    Title = "Out My Mind",
                    Artists = context.Artists.Where(a => a.Name.Equals("Tritonal")).ToList()
                },
                new Song
                {
                    Title = "Out My Mind (Apek Remix)",
                    Artists = artists.Skip(2).ToList()
                }
            };

            foreach (var s in songs)
            {
                context.Songs.Add(s);
            }

            context.SaveChanges();
        }
    }
}