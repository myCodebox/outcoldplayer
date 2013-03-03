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
        private readonly Playlist playlist;

        public PlaylistBindingModel(Playlist playlist)
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
                return this.playlist is Album;
            }
        }

        public Playlist Playlist
        {
            get
            {
                return this.playlist;
            }
        }
    }
}