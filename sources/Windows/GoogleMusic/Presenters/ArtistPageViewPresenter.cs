// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class ArtistPageViewPresenter : PagePresenterBase<IArtistPageView, ArtistPageViewBindingModel>
    {
        private readonly IApplicationResources resources;
        private readonly IPlayQueueService playQueueService;
        private readonly INavigationService navigationService;
        private readonly IPlaylistsService playlistsService;
        private readonly IAlbumsRepository albumsRepository;
        private readonly ISongsCachingService cachingService;
        private readonly IApplicationStateService stateService;

        internal ArtistPageViewPresenter(
            IApplicationResources resources,
            IPlayQueueService playQueueService,
            INavigationService navigationService,
            IPlaylistsService playlistsService,
            IAlbumsRepository albumsRepository,
            ISongsCachingService cachingService,
            IApplicationStateService stateService)
        {
            this.resources = resources;
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;
            this.albumsRepository = albumsRepository;
            this.cachingService = cachingService;
            this.stateService = stateService;
            this.PlayCommand = new DelegateCommand(this.Play);
            this.ShowAllCommand = new DelegateCommand(this.ShowAll);

            this.QueueCommand = new DelegateCommand(this.Queue, () => this.BindingModel.SelectedItems.Count > 0);
            this.DownloadCommand = new DelegateCommand(this.Download, () => this.BindingModel.SelectedItems.Count > 0);
        }
        
        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand ShowAllCommand { get; set; }

        public DelegateCommand QueueCommand { get; private set; }

        public DelegateCommand DownloadCommand { get; set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.Artist = null;
            this.BindingModel.Albums = null;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
            if (request == null || request.PlaylistType != PlaylistType.Artist)
            {
                throw new NotSupportedException("Request should be PlaylistNavigationRequest and playlist type should be artist.");
            }

            this.BindingModel.Artist = await this.playlistsService.GetRepository<Artist>().GetAsync(request.PlaylistId);
            this.BindingModel.Albums = (await this.albumsRepository.GetArtistAlbumsAsync(request.PlaylistId))
                    .Select(a => new PlaylistBindingModel(a) { PlayCommand = this.PlayCommand })
                    .ToList();
        }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield return new CommandMetadata(CommandIcon.List, this.resources.GetString("Toolbar_ShowAllButton"), this.ShowAllCommand);
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnSelectedItemsChanged();
        }

        private void OnSelectedItemsChanged()
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

        private IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.OpenWith, this.resources.GetString("Toolbar_QueueButton"), this.QueueCommand);
            if (this.stateService.IsOnline())
            {
                yield return new CommandMetadata(CommandIcon.Download, this.resources.GetString("Toolbar_KeepLocal"), this.DownloadCommand);
            }
        }

        private void ShowAll()
        {
            this.NavigateToShowAllArtistsSongs();
        }

        private void Play(object commandParameter)
        {
            if (this.BindingModel.Artist != null)
            {
                IPlaylist playlist = commandParameter as Album;
                if (playlist != null)
                {
                    this.navigationService.NavigateToPlaylist(playlist);
                    this.playQueueService.PlayAsync(playlist);
                    this.MainFrame.IsBottomAppBarOpen = true;
                }
            }
        }

        private void NavigateToShowAllArtistsSongs()
        {
            if (this.BindingModel.Artist != null)
            {
                this.navigationService.NavigateTo<IPlaylistPageView>(new PlaylistNavigationRequest(PlaylistType.Artist, this.BindingModel.Artist.Id));
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
    }
}