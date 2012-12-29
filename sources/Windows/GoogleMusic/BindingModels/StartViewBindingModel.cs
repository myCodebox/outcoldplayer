// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.ObjectModel;

    public class StartViewBindingModel : BindingModelBase
    {
        private bool isLoadingPlaylists;
        private bool isLoadingAlbums;

        private int playlistsCount;
        private int albumsCount;

        public StartViewBindingModel()
        {
            this.IsLoadingPlaylists = true;
            this.IsLoadingAlbums = true;
            this.Playlists = new ObservableCollection<PlaylistBindingModel>();
            this.Albums = new ObservableCollection<PlaylistBindingModel>();
        }

        public ObservableCollection<PlaylistBindingModel> Playlists { get; private set; }

        public ObservableCollection<PlaylistBindingModel> Albums { get; private set; }

        public int PlaylistsCount
        {
            get
            {
                return this.playlistsCount;
            }

            set
            {
                this.playlistsCount = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public int AlbumsCount
        {
            get
            {
                return this.albumsCount;
            }

            set
            {
                this.albumsCount = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public bool IsLoadingPlaylists
        {
            get
            {
                return this.isLoadingPlaylists;
            }

            set
            {
                if (this.isLoadingPlaylists != value)
                {
                    this.isLoadingPlaylists = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public bool IsLoadingAlbums
        {
            get
            {
                return this.isLoadingAlbums;
            }

            set
            {
                if (this.isLoadingAlbums != value)
                {
                    this.isLoadingAlbums = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }
    }
}