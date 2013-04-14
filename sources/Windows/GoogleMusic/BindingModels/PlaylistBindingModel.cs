// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistBindingModel 
    {
        private readonly IPlaylist playlist;

        public PlaylistBindingModel(IPlaylist playlist)
        {
            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            this.playlist = playlist;
        }

        public DelegateCommand PlayCommand { get; set; }

        public IPlaylist Playlist
        {
            get
            {
                return this.playlist;
            }
        }
    }
}