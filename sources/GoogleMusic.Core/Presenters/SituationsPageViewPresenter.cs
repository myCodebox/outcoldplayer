// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class SituationsPageViewPresenter : PagePresenterBase<ISituationsPageView, SituationsPageViewBindingModel>
    {
        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.Title = null;
            this.BindingModel.Subtitle = null;
            this.BindingModel.Playlists = null;
            this.BindingModel.SituationGroup = null;
        }

        protected override Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var playlistNavigationRequest = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
                if (playlistNavigationRequest != null)
                {
                    var situationGroup = playlistNavigationRequest.Playlist as SituationGroup;
                    if (situationGroup != null)
                    {
                        await this.Dispatcher.RunAsync(() =>
                        {
                            this.BindingModel.Title = situationGroup.Title;
                            this.BindingModel.Subtitle = situationGroup.Description;
                            this.BindingModel.PlaylistType = PlaylistType.SituationStations;
                            this.BindingModel.Playlists = situationGroup.Situations.Cast<IPlaylist>().ToList();
                            this.BindingModel.SituationGroup = situationGroup;
                        });
                    }
                }
            }, cancellationToken);
        }
    }
}
