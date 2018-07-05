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

        public Artist Artist { get; set; }

        //public List<SongUri> Urls { get; set; }
    }
}