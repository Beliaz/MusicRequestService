using System.Collections.Generic;

namespace BotService
{
    public class PlaySongState : BaseState
    {
        public bool HasTitle => string.IsNullOrEmpty(Title);

        public bool HasArtist => string.IsNullOrEmpty(Artist);

        public bool HasProvider => string.IsNullOrEmpty(Provider);

        public string Title
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string Artist
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string Provider
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public PlaySongState(IDictionary<string, object> source) : base(source)
        {
        }
    }
}