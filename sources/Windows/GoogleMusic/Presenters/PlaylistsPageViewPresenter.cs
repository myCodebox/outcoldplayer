// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistsPageViewPresenter : PlaylistsPageViewPresenterBase<IPlaylistsPageView, PlaylistsPageViewBindingModel>
    {
        public PlaylistsPageViewPresenter(
            IApplicationResources resources,
            IPlaylistsService playlistsService)
            : base(resources, playlistsService)
        {
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
            if (request != null)
            {
                this.BindingModel.Title = request.Title;
                this.BindingModel.Subtitle = request.Subtitle;
                if (request.Playlists.Count > 0)
                {
                    this.BindingModel.PlaylistType = request.Playlists.Select(x => x.PlaylistType).First();
                }

                this.BindingModel.Playlists = request.Playlists;
            }
            else
            {
                await base.LoadDataAsync(navigatedToEventArgs);
            }
        }
    }
}