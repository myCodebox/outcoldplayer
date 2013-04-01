// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistsPageViewPresenter : PlaylistsPageViewPresenterBase<IPlaylistsPageView, PlaylistsPageViewBindingModel>
    {
        public PlaylistsPageViewPresenter(
            IPlaylistsService playlistsService,
            INavigationService navigationService,
            IPlayQueueService playQueueService)
            : base(playlistsService, navigationService, playQueueService)
        {
        }
    }
}