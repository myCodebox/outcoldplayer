// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public interface IPlaylistsPageViewPresenterBase
    {
        bool IsMixedList { get; }
    }

    public abstract class PlaylistsPageViewPresenterBase<TView> : PagePresenterBase<TView, PlaylistsPageViewBindingModel>, IPlaylistsPageViewPresenterBase
        where TView : IPageView
    {
        private readonly IPlaylistsService playlistsService;

        private IDisposable playlistsChangeSubscription;

        protected PlaylistsPageViewPresenterBase(
            IPlaylistsService playlistsService)
        {
            this.playlistsService = playlistsService;
            this.IsMixedList = false;
        }

        public bool IsMixedList { get; protected set; }

        public override void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
            base.OnNavigatedTo(parameter);

            if (this.playlistsChangeSubscription != null)
            {
                this.playlistsChangeSubscription.Dispose();
            }

            this.playlistsChangeSubscription = this.EventAggregator.GetEvent<PlaylistsChangeEvent>()
                                                    .Where(e => !(parameter.Parameter is PlaylistType) || e.PlaylistType == (PlaylistType)parameter.Parameter)
                                                    .Subscribe((e) => this.Logger.LogTask(this.LoadPlaylistsAsync()));
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.Title = null;
            this.BindingModel.Subtitle = null;
            this.BindingModel.Playlists = null;

            if (this.playlistsChangeSubscription != null)
            {
                this.playlistsChangeSubscription.Dispose();
                this.playlistsChangeSubscription = null;
            }
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken)
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
                this.BindingModel.PlaylistType = (PlaylistType)navigatedToEventArgs.Parameter;
                this.BindingModel.Title = PlaylistTypeEx.GetPluralTitle(this.BindingModel.PlaylistType);

                await this.LoadPlaylistsAsync();
            }
        }

        protected async virtual Task LoadPlaylistsAsync()
        {
            var playlists = await this.playlistsService.GetAllAsync(this.BindingModel.PlaylistType, Order.Name);

            await this.Dispatcher.RunAsync(() =>
            {
                this.BindingModel.Playlists = playlists.ToList();
                this.BindingModel.Subtitle = this.BindingModel.Playlists == null ? string.Empty : this.BindingModel.Playlists.Count.ToString();
            });
        }
    }
}