using System.Collections.Generic;

namespace SoundPool.Data
{
    public class Song
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public ICollection<Artist> Artists { get; set; }

        public string Url { get; set; }
    }
}