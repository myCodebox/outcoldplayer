// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    public class HomePageViewBindingModel : PlaylistsPageViewBindingModel
    {
        private string situationHeader;
        private IList<SituationGroup> situations;
        private IList<IPlaylist> systemPlaylists;

        public string SituationHeader
        {
            get { return this.situationHeader; }
            set { this.SetValue(ref this.situationHeader, value); }
        }

        public IList<SituationGroup> Situations
        {
            get { return this.situations; }
            set { this.SetValue(ref this.situations, value); }
        }

        public IList<IPlaylist> SystemPlaylists
        {
            get { return this.systemPlaylists; }
            set { this.SetValue(ref this.systemPlaylists, value); }
        }
    }
}
