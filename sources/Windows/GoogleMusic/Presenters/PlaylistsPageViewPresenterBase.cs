// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public abstract class PlaylistsPageViewPresenterBase<TView, TPlaylistsPageViewBindingModel> : PagePresenterBase<TView, TPlaylistsPageViewBindingModel>
        where TView : IPageView
        where TPlaylistsPageViewBindingModel : PlaylistsPageViewBindingModel
    {
        private readonly IApplicationResources resources;
        private readonly IPlaylistsService playlistsService;

        private IDisposable playlistsChangeSubscription;

        protected PlaylistsPageViewPresenterBase(
            IApplicationResources resources,
            IPlaylistsService playlistsService)
        {
            this.resources = resources;
            this.playlistsService = playlistsService;
        }

        public override void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
            base.OnNavigatedTo(parameter);

            this.playlistsChangeSubscription = this.EventAggregator.GetEvent<PlaylistsChangeEvent>()
                                                    .Where(e => e.PlaylistType == (PlaylistType)parameter.Parameter)
                                                    .Subscribe((e) => this.Logger.LogTask(this.LoadPlaylistsAsync()));
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            if (this.playlistsChangeSubscription != null)
            {
                this.playlistsChangeSubscription.Dispose();
                this.playlistsChangeSubscription = null;
            }
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            this.BindingModel.PlaylistType = (PlaylistType)navigatedToEventArgs.Parameter;
            this.BindingModel.Title = this.resources.GetPluralTitle(this.BindingModel.PlaylistType);
            await this.LoadPlaylistsAsync();
        }

        protected async virtual Task LoadPlaylistsAsync()
        {
            var playlists = await this.playlistsService.GetAllAsync(this.BindingModel.PlaylistType, Order.Name);

            await this.Dispatcher.RunAsync(() =>
            {
                this.BindingModel.Playlists = playlists.ToList();
            });
        }
    }
}