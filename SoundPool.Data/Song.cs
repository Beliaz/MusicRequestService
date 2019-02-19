using System.Collections.Generic;

namespace SoundPool.Data
{
    public class Song
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public ICollection<SongArtist> Artists { get; set; } = new List<SongArtist>();

        public ICollection<SongUrl> Url { get; set; } = new List<SongUrl>();
    }
}