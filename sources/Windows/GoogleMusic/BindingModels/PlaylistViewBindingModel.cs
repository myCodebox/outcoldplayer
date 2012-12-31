// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistViewBindingModel : SongsBindingModelBase
    {
        private readonly Playlist playlist;

        public PlaylistViewBindingModel(Playlist playlist)
        {
            this.playlist = playlist;

            if (this.playlist.Songs != null)
            {
                foreach (var song in this.playlist.Songs)
                {
                    this.Songs.Add(song);
                }
            }
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