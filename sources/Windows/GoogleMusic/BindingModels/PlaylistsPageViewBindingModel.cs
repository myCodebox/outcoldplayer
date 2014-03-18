// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistsPageViewBindingModel : BindingModelBase
    {
        private IList<IPlaylist> playlists;

        private string title;
        private string subtitle;
        private PlaylistType playlistType;

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.SetValue(ref this.title, value);
            }
        }

        public string Subtitle
        {
            get
            {
                return this.subtitle;
            }

            set
            {
                this.SetValue(ref this.subtitle, value);
            }
        }

        public PlaylistType PlaylistType
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

        public IList<IPlaylist> Playlists
        {
            get
            {
                return this.playlists;
            }

            set
            {
                if (this.SetValue(ref this.playlists, value))
                {
                    this.RaisePropertyChanged(() => this.Subtitle);
                }
            }
        }
    }
}