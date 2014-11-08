// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Services;

    public class SituationStationsPageViewBindingModel : PlaylistsPageViewBindingModel
    {
        private SituationStations situationStations;

        public SituationStations SituationStations
        {
            get { return this.situationStations; }
            set { this.SetValue(ref this.situationStations, value); }
        }
    }
}
