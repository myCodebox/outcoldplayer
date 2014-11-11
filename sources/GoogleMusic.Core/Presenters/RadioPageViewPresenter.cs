// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class RadioPageViewPresenter : PlaylistsPageViewPresenterBase<IRadioPageView, PlaylistsPageViewBindingModel>
    {
        public RadioPageViewPresenter(
            IPlaylistsService playlistsService)
            : base(playlistsService)
        {
        }
    }
}
