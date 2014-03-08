// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistsPageViewBindingModel : BindingModelBase
    {
        private IList<IPlaylist> playlists;
        private IList<PlaylistsGroupBindingModel> groups;

        private string title;
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
                return this.Playlists == null ? string.Empty : this.Playlists.Count.ToString();
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
                    this.RecalculateGroups();
                    this.RaisePropertyChanged(() => this.Subtitle);
                }
            }
        }

        public IList<PlaylistsGroupBindingModel> Groups
        {
            get
            {
                return this.groups;
            }

            set
            {
                this.SetValue(ref this.groups, value);
            }
        }

        private void RecalculateGroups()
        {
            if (this.Playlists == null)
            {
                this.Groups = null;
            }
            else
            {
                this.Groups = this.Playlists.GroupBy(p => p.Title.Length > 0 ? char.ToUpper(p.Title[0]) : ' ')
                                                        .Select(g => new PlaylistsGroupBindingModel(g.Key.ToString(), g.Count(), g.Select(x => new PlaylistBindingModel(x)).ToList()))
                                                        .ToList();
            }
        }
    }
}