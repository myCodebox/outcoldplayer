// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistsPageViewPresenter : PlaylistsPageViewPresenterBase<IPlaylistsPageView>
    {
        public PlaylistsPageViewPresenter(
            IPlaylistsService playlistsService)
            : base(playlistsService)
        {
        }
    }
}