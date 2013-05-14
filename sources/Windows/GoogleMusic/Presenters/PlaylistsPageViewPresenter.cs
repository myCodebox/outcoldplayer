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
            IApplicationResources resources,
            IPlaylistsService playlistsService,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            ISongsCachingService cachingService,
            IApplicationStateService stateService)
            : base(resources, playlistsService, navigationService, playQueueService, cachingService, stateService)
        {
        }
    }
}