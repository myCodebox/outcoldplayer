// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class PlaylistBindingModel
    {
        private readonly GoogleMusicPlaylist playlist;

        public PlaylistBindingModel(GoogleMusicPlaylist playlist)
        {
            this.playlist = playlist;
        }

        public string Title
        {
            get
            {
                return this.playlist.Title;
            }
        }
    }
}