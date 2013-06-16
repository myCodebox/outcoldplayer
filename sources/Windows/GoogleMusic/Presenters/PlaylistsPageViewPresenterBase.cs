// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public abstract class PlaylistsPageViewPresenterBase<TView, TPlaylistsPageViewBindingModel> : PagePresenterBase<TView, TPlaylistsPageViewBindingModel>
        where TView : IPageView
        where TPlaylistsPageViewBindingModel : PlaylistsPageViewBindingModel
    {
        private readonly IApplicationResources resources;
        private readonly IPlaylistsService playlistsService;
        private readonly INavigationService navigationService;
        private readonly IPlayQueueService playQueueService;
        private readonly ISongsCachingService cachingService;
        private readonly IApplicationStateService stateService;

        private IDisposable playlistsChangeSubscription;

        protected PlaylistsPageViewPresenterBase(
            IApplicationResources resources,
            IPlaylistsService playlistsService,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            ISongsCachingService cachingService,
            IApplicationStateService stateService)
        {
            this.resources = resources;
            this.playlistsService = playlistsService;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;
            this.cachingService = cachingService;
            this.stateService = stateService;

            this.PlayCommand = new DelegateCommand(this.Play);
            this.QueueCommand = new DelegateCommand(this.Queue, () => this.BindingModel.SelectedItems.Count > 0);
            this.DownloadCommand = new DelegateCommand(this.Download, () => this.BindingModel.SelectedItems.Count > 0);
            this.UnPinCommand = new DelegateCommand(this.UnPin, () => this.BindingModel.SelectedItems.Count > 0);
        }

        public DelegateCommand PlayCommand { get; private set; }

        public DelegateCommand QueueCommand { get; private set; }

        public DelegateCommand DownloadCommand { get; private set; }

        public DelegateCommand UnPinCommand { get; private set; }

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

        protected virtual void OnSelectedItemsChanged()
        {
            if (this.BindingModel.SelectedItems.Count > 0)
            {
                this.MainFrame.SetContextCommands(this.GetContextCommands());
            }
            else
            {
                this.MainFrame.ClearContextCommands();
            }
        }

        protected virtual IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.OpenWith, this.resources.GetString("Toolbar_QueueButton"), this.QueueCommand);
            if (this.stateService.IsOnline() && 
                (this.BindingModel.SelectedItems.Any(x => x.Playlist.OfflineSongsCount != x.Playlist.SongsCount)
                || this.BindingModel.SelectedItems.All(x => x.Playlist.SongsCount == 0)))
            {
                yield return new CommandMetadata(CommandIcon.Pin, this.resources.GetString("Toolbar_KeepLocal"), this.DownloadCommand);
            }
            else
            {
                yield return new CommandMetadata(CommandIcon.UnPin, this.resources.GetString("Toolbar_RemoveLocal"), this.UnPinCommand);
            }
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnSelectedItemsChanged();
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

        private void Queue()
        {
            this.MainFrame.ShowPopup<IQueueActionsPopupView>(
                PopupRegion.AppToolBarLeft,
                new SelectedItems(this.BindingModel.SelectedItems.Select(bm => bm.Playlist).ToList())).Closed += this.QueueActionsPopupView_Closed;
        }

        private void QueueActionsPopupView_Closed(object sender, EventArgs eventArgs)
        {
            ((IPopupView)sender).Closed -= this.QueueActionsPopupView_Closed;
            if (eventArgs is QueueActionsCompletedEventArgs)
            {
                this.BindingModel.ClearSelectedItems();
            }
        }

        private async void Download()
        {
            try
            {
                IEnumerable<Song> songs = Enumerable.Empty<Song>();

                foreach (var playlistBindingModel in this.BindingModel.SelectedItems)
                {
                    songs = songs.Union(await this.playlistsService.GetSongsAsync(playlistBindingModel.Playlist));
                }

                await this.cachingService.QueueForDownloadAsync(songs);
                this.BindingModel.ClearSelectedItems();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot add songs to download queue");
            }
        }

        private async void UnPin()
        {
            try
            {
                IEnumerable<Song> songs = Enumerable.Empty<Song>();

                foreach (var playlistBindingModel in this.BindingModel.SelectedItems)
                {
                    songs = songs.Union(await this.playlistsService.GetSongsAsync(playlistBindingModel.Playlist));
                }

                await this.cachingService.ClearCachedAsync(songs);
                this.BindingModel.ClearSelectedItems();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot remove from cache selected songs.");
            }
        }
    }
}