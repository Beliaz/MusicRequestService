using System.Collections.Generic;

namespace SoundPool.Data
{
    public class SongUri
    {
        public string Id { get; set; }

        public string UriString { get; set; }
    }

    public class Song
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public ICollection<Artist> Artists { get; set; }

        //public List<SongUri> Urls { get; set; }
    }
}