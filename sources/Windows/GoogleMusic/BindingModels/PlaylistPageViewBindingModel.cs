// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistPageViewBindingModel<TPlaylist> : BindingModelBase where TPlaylist : class, IPlaylist
    {
        private TPlaylist playlist;

        private string playlistType;

        public PlaylistPageViewBindingModel(
            SongsBindingModel songsBindingModel)
        {
            this.SongsBindingModel = songsBindingModel;
        }

        public TPlaylist Playlist
        {
            get
            {
                return this.playlist;
            }

            set
            {
                this.SetValue(ref this.playlist, value);
                this.RaisePropertyChanged(() => this.Type);
            }
        }

        public SongsBindingModel SongsBindingModel { get; private set; }

        public string Type
        {
            get
            {
                return this.playlistType;
            }

            set
            {
                this.SetValue(ref this.playlistType, value);
            }
        }
    }
}