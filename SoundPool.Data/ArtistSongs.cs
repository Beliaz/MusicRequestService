using System.Collections.Generic;

namespace SoundPool.Data
{
    public class ArtistSongs
    {
        public string ArtistId { get; set; }

        public Artist Artist { get; set; }

        public ICollection<Song> Songs { get; set; }
    }
}