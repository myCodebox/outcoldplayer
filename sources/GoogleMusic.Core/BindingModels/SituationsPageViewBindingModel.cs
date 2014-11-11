// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Services;

    public class SituationsPageViewBindingModel : PlaylistsPageViewBindingModel
    {
        private SituationGroup situationGroup;

        public SituationGroup SituationGroup
        {
            get { return this.situationGroup; }
            set { this.SetValue(ref this.situationGroup, value); }
        }
    }
}
