// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistPageViewBindingModel<TPlaylist> : BindingModelBase where TPlaylist : Playlist
    {
        private TPlaylist playlist;
        private int selectedSongIndex = -1;

        public TPlaylist Playlist
        {
            get
            {
                return this.playlist;
            }

            set
            {
                this.playlist = value;
                this.RaiseCurrentPropertyChanged();
                this.SelectedSongIndex = -1;
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

        public Song SelectedSong
        {
            get
            {
                if (this.playlist.Songs.Count > this.SelectedSongIndex && this.SelectedSongIndex > 0)
                {
                    return this.playlist.Songs[this.SelectedSongIndex];
                }

                return null;
            }
        }

        public int SelectedSongIndex
        {
            get
            {
                return this.selectedSongIndex;
            }

            set
            {
                this.selectedSongIndex = value;
                this.RaiseCurrentPropertyChanged();
                this.RaisePropertyChanged(() => this.SelectedSong);
            }
        }
    }
}