using System.Collections.Generic;

namespace SoundPool.Data
{
    public class Artist
    {
        public string Name { get; set; }

        public ICollection<SongArtist> Songs { get; } = new List<SongArtist>();
    }
}