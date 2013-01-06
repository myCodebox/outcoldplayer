// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistViewBindingModel : SongsBindingModelBase
    {
        private readonly Playlist playlist;
        private bool isBusy = false;
        private bool isLoading = false;

        public PlaylistViewBindingModel(Playlist playlist)
        {
            this.playlist = playlist;

            this.ReloadSongs();
        }

        public string Title
        {
            get
            {
                return this.playlist.Title;
            }
        }

        public string Type
        {
            get
            {
                if (this.playlist is Album)
                {
                    return "Album";
                }

                if (this.playlist is Artist)
                {
                    return "Artist";
                }

                if (this.playlist is Genre)
                {
                    return "Genre";
                }

                if (this.playlist is MusicPlaylist)
                {
                    return "Playlist";
                }

                return null;
            }
        }

        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }

            set
            {
                this.isBusy = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.isLoading = value;
                this.RaiseCurrentPropertyChanged();
            }
        }


        public Playlist Playlist
        {
            get
            {
                return this.playlist;
            }
        }

        public void ReloadSongs()
        {
            if (this.playlist.Songs != null)
            {
                this.Songs.Clear();
                foreach (var song in this.playlist.Songs)
                {
                    this.Songs.Add(song);
                }
            }
        }
    }
}