// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistBindingModel : BindingModelBase
    {
        private readonly PlaylistBaseBindingModel playlist;

        public PlaylistBindingModel(PlaylistBaseBindingModel playlist)
        {
            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            this.playlist = playlist;
        }

        public DelegateCommand PlayCommand { get; set; }

        public bool IsAlbum
        {
            get
            {
                return this.playlist is AlbumBindingModel;
            }
        }

        public PlaylistBaseBindingModel Playlist
        {
            get
            {
                return this.playlist;
            }
        }
    }
}