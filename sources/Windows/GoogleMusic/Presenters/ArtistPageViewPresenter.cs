// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class ArtistPageViewPresenter : PagePresenterBase<IArtistPageView, ArtistPageViewBindingModel>
    {
        private readonly IPlayQueueService playQueueService;
        private readonly INavigationService navigationService;
        private readonly IPlaylistsService playlistsService;
        private readonly IAlbumsRepository albumsRepository;

        public ArtistPageViewPresenter(
            IPlayQueueService playQueueService,
            INavigationService navigationService,
            IPlaylistsService playlistsService,
            IAlbumsRepository albumsRepository)
        {
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;
            this.albumsRepository = albumsRepository;
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
            yield return new CommandMetadata(CommandIcon.Play, "Play All", this.PlayCommand);
            yield return new CommandMetadata(CommandIcon.List, "Show All", this.ShowAllCommand);

            // TODO: Will be good to have shuffle all 
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
                if (playlist == null)
                {
                    playlist = this.BindingModel.Artist;
                    this.NavigateToShowAllArtistsSongs();
                }
                else
                {
                    this.navigationService.NavigateToPlaylist(playlist);
                }

                this.playQueueService.PlayAsync(playlist);
                this.MainFrame.IsBottomAppBarOpen = true;
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