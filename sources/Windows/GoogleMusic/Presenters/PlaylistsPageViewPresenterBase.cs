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
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public abstract class PlaylistsPageViewPresenterBase<TView, TPlaylistsPageViewBindingModel> : PagePresenterBase<TView, TPlaylistsPageViewBindingModel>
        where TView : IPageView
        where TPlaylistsPageViewBindingModel : PlaylistsPageViewBindingModel
    {
        private readonly IPlaylistsService playlistsService;
        private readonly INavigationService navigationService;
        private readonly IPlayQueueService playQueueService;

        private IDisposable playlistsChangeSubscription;

        protected PlaylistsPageViewPresenterBase(
            IPlaylistsService playlistsService,
            INavigationService navigationService,
            IPlayQueueService playQueueService)
        {
            this.playlistsService = playlistsService;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;

            this.PlayCommand = new DelegateCommand(this.Play);
            this.AddToQueueCommand = new DelegateCommand(this.AddToQueue, () => this.BindingModel.SelectedItems.Count > 0);
        }

        public DelegateCommand PlayCommand { get; private set; }

        public DelegateCommand AddToQueueCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
            base.OnNavigatedTo(parameter);

            this.playlistsChangeSubscription = this.EventAggregator.GetEvent<PlaylistsChangeEvent>()
                                                    .Where(e => e.PlaylistType == (PlaylistType)parameter.Parameter)
                                                    .Subscribe((e) => this.Logger.LogTask(this.LoadPlaylists()));
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

        protected override Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            this.BindingModel.PlaylistType = (PlaylistType)navigatedToEventArgs.Parameter;
            return this.LoadPlaylists();
        }

        protected async virtual Task LoadPlaylists()
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
            yield return new CommandMetadata(CommandIcon.Add, "Queue", this.AddToQueueCommand);
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
                this.navigationService.NavigateToPlaylist(playlist);
                this.playQueueService.PlayAsync(playlist);
                this.MainFrame.IsBottomAppBarOpen = true;
            }
        }

        private async void AddToQueue()
        {
            try
            {
                List<IPlaylist> selectedPlaylists = this.BindingModel.SelectedItems.Select(bm => bm.Playlist).ToList();
                List<Song> songs = new List<Song>(selectedPlaylists.Sum(p => p.SongsCount));
                foreach (var selectedPlaylist in selectedPlaylists)
                {
                    songs.AddRange(await this.playlistsService.GetSongsAsync(selectedPlaylist));
                }

                await this.playQueueService.AddRangeAsync(songs);
                await this.Dispatcher.RunAsync(() => this.BindingModel.ClearSelectedItems());
            }
            catch (Exception e)
            {
                this.Logger.LogErrorException(e);
            }
        }
    }
}