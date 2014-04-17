// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class RadioPageViewPresenter : PlaylistsPageViewPresenterBase<IRadioPageView>
    {
        public RadioPageViewPresenter(
            IApplicationResources resources, 
            IPlaylistsService playlistsService)
            : base(resources, playlistsService)
        {
        }
    }
}
