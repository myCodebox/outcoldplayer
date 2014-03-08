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

    public class ArtistPageViewPresenter : PagePresenterBase<IArtistPageView, ArtistPageViewBindingModel>
    {
        private readonly IApplicationResources resources;
        private readonly IPlayQueueService playQueueService;
        private readonly INavigationService navigationService;
        private readonly IPlaylistsService playlistsService;
        private readonly IAlbumsRepository albumsRepository;
        private readonly ISelectedObjectsService selectedObjectsService;

        internal ArtistPageViewPresenter(
            IApplicationResources resources,
            IPlayQueueService playQueueService,
            INavigationService navigationService,
            IPlaylistsService playlistsService,
            IAlbumsRepository albumsRepository,
            ISelectedObjectsService selectedObjectsService)
        {
            this.resources = resources;
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;
            this.albumsRepository = albumsRepository;
            this.selectedObjectsService = selectedObjectsService;
            this.PlayCommand = new DelegateCommand(this.Play);
            this.ShowAllCommand = new DelegateCommand(this.ShowAll);
        }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand ShowAllCommand { get; set; }

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

            this.EventAggregator.GetEvent<SelectionClearedEvent>()
               .Subscribe<SelectionClearedEvent>(async (e) => await this.Dispatcher.RunAsync(() => this.BindingModel.ClearSelectedItems()));
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
            yield return new CommandMetadata(CommandSymbol.List, this.resources.GetString("Toolbar_ShowAllButton"), this.ShowAllCommand);
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.selectedObjectsService.Update(
                e.NewItems == null ? null : e.NewItems.Cast<PlaylistBindingModel>().Select(x => x.Playlist),
                e.OldItems == null ? null : e.OldItems.Cast<PlaylistBindingModel>().Select(x => x.Playlist));
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
    }
}