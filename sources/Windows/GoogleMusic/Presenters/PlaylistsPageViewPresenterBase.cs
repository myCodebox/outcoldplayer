// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Specialized;
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
        private readonly INavigationService navigationService;
        private readonly IPlayQueueService playQueueService;
        private readonly ISelectedObjectsService selectedObjectsService;

        private IDisposable playlistsChangeSubscription;

        protected PlaylistsPageViewPresenterBase(
            IApplicationResources resources,
            IPlaylistsService playlistsService,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            ISelectedObjectsService selectedObjectsService)
        {
            this.resources = resources;
            this.playlistsService = playlistsService;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;
            this.selectedObjectsService = selectedObjectsService;

            this.PlayCommand = new DelegateCommand(this.Play);
        }

        public DelegateCommand PlayCommand { get; private set; }

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

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;
            this.EventAggregator.GetEvent<SelectionClearedEvent>()
                .Subscribe<SelectionClearedEvent>(async(e) => await this.Dispatcher.RunAsync(() => this.BindingModel.ClearSelectedItems()));
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            this.BindingModel.PlaylistType = (PlaylistType)navigatedToEventArgs.Parameter;
            this.BindingModel.Title = this.resources.GetPluralTitle(this.BindingModel.PlaylistType);
            await this.LoadPlaylistsAsync();
            await this.Dispatcher.RunAsync(() => this.BindingModel.ClearSelectedItems());
        }

        protected async virtual Task LoadPlaylistsAsync()
        {
            var playlists = (await this.playlistsService.GetAllAsync(this.BindingModel.PlaylistType, Order.Name))
                                .Select(playlist => new PlaylistBindingModel(playlist) { PlayCommand = this.PlayCommand });

            await this.Dispatcher.RunAsync(() =>
            {
                this.BindingModel.Playlists = playlists.ToList();
            });
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.selectedObjectsService.Update(
                e.NewItems == null ? null : e.NewItems.Cast<PlaylistBindingModel>().Select(x => x.Playlist),
                e.OldItems == null ? null : e.OldItems.Cast<PlaylistBindingModel>().Select(x => x.Playlist)); 
        }

        private void Play(object commandParameter)
        {
            IPlaylist playlist = commandParameter as IPlaylist;
            if (playlist != null)
            {
                this.Logger.LogTask(this.playQueueService.PlayAsync(playlist));
                this.navigationService.NavigateTo<ICurrentPlaylistPageView>();
                this.MainFrame.IsBottomAppBarOpen = true;
            }
        }
    }
}