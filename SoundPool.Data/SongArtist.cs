using System.Collections.Generic;

namespace SoundPool.Data
{
    public class SongArtist
    {
        public string Id { get; set; }

        public string SongId { get; set; }

        public Song Song { get; set; }

        public string ArtistId { get; set; }

        public Artist Artist { get; set; }
    }
}