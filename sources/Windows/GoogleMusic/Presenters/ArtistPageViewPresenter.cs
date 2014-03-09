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

    public class ArtistPageViewPresenter : PagePresenterBase<IArtistPageView, ArtistPageViewBindingModel>
    {
        private readonly IApplicationResources resources;
        private readonly IPlayQueueService playQueueService;
        private readonly INavigationService navigationService;
        private readonly IPlaylistsService playlistsService;
        private readonly IAlbumsRepository albumsRepository;
        private readonly IArtistsRepository artistsRepository;
        private readonly IRadioStationsService radioStationsService;
        private readonly IApplicationStateService applicationStateService;
        private readonly IAllAccessService allAccessService;

        internal ArtistPageViewPresenter(
            IApplicationResources resources,
            IPlayQueueService playQueueService,
            INavigationService navigationService,
            IPlaylistsService playlistsService,
            IAlbumsRepository albumsRepository,
            IArtistsRepository artistsRepository,
            IRadioStationsService radioStationsService,
            IApplicationStateService applicationStateService,
            IAllAccessService allAccessService)
        {
            this.resources = resources;
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;
            this.albumsRepository = albumsRepository;
            this.artistsRepository = artistsRepository;
            this.radioStationsService = radioStationsService;
            this.applicationStateService = applicationStateService;
            this.allAccessService = allAccessService;
            this.ShowAllCommand = new DelegateCommand(this.ShowAll);
            this.StartRadioCommand = new DelegateCommand(this.StartRadio);
        }

        public DelegateCommand ShowAllCommand { get; set; }

        public DelegateCommand StartRadioCommand { get; set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.Artist = null;
            this.BindingModel.Albums = null;
            this.BindingModel.Collections = null;
            this.BindingModel.TopSongs = null;
            this.BindingModel.GoogleMusicAlbums = null;
            this.BindingModel.RelatedArtists = null;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
            if (request == null || request.PlaylistType != PlaylistType.Artist)
            {
                throw new NotSupportedException("Request should be PlaylistNavigationRequest and playlist type should be artist.");
            }

            Artist artist = request.Playlist as Artist;

            if (artist != null && artist.ArtistId == 0)
            {
                artist = (await this.artistsRepository.FindByGoogleIdAsync(artist.GoogleArtistId)) ?? artist;
            }
            else
            {
                artist = await this.playlistsService.GetRepository<Artist>().GetAsync(request.PlaylistId);
            }

            if (artist.ArtistId > 0)
            {
                this.BindingModel.Artist = artist;
                this.BindingModel.Albums = (await this.albumsRepository.GetArtistAlbumsAsync(artist.Id)).Cast<IPlaylist>().ToList();
                this.BindingModel.Collections = (await this.albumsRepository.GetArtistCollectionsAsync(artist.Id)).Cast<IPlaylist>().ToList();
            }

            if (this.applicationStateService.IsOnline())
            {
                var info = await this.allAccessService.GetArtistInfoAsync(artist);
                if (info != null)
                {
                    this.BindingModel.Artist = info.Artist;
                    this.BindingModel.TopSongs = info.TopSongs;
                    this.BindingModel.GoogleMusicAlbums = info.GoogleAlbums == null ? null : info.GoogleAlbums.Cast<IPlaylist>().ToList();
                    this.BindingModel.RelatedArtists = info.RelatedArtists == null ? null : info.RelatedArtists.Cast<IPlaylist>().ToList();
                }
            }
        }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            if (this.BindingModel.Artist.ArtistId > 0)
            { 
                yield return new CommandMetadata(CommandSymbol.List, this.resources.GetString("Toolbar_ShowAllButton"), this.ShowAllCommand);
            }

            if (this.applicationStateService.IsOnline())
            {
                yield return new CommandMetadata(CommandSymbol.Radio, "Start radio", this.StartRadioCommand);
            }
        }

        private void ShowAll()
        {
            this.NavigateToShowAllArtistsSongs();
        }

        private async void StartRadio()
        {
            if (!this.IsDataLoading)
            {
                await this.Dispatcher.RunAsync(() => this.IsDataLoading = true);

                var radio = await this.radioStationsService.CreateAsync(this.BindingModel.Artist);

                if (radio != null)
                {
                    await this.playQueueService.PlayAsync(radio.Item1, radio.Item2, -1);

                    await this.Dispatcher.RunAsync(() => this.IsDataLoading = false);

                    this.navigationService.NavigateTo<ICurrentPlaylistPageView>();
                }
            }
        }

        private void NavigateToShowAllArtistsSongs()
        {
            if (this.BindingModel.Artist != null)
            {
                this.navigationService.NavigateTo<IPlaylistPageView>(this.BindingModel.Artist);
            }
        }
    }
}