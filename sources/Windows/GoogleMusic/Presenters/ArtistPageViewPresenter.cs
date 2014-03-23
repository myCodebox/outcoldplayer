// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

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
        private readonly ISettingsService settingsService;

        private readonly IAnalyticsService analyticsService;

        internal ArtistPageViewPresenter(
            IApplicationResources resources,
            IPlayQueueService playQueueService,
            INavigationService navigationService,
            IPlaylistsService playlistsService,
            IAlbumsRepository albumsRepository,
            IArtistsRepository artistsRepository,
            IRadioStationsService radioStationsService,
            IApplicationStateService applicationStateService,
            IAllAccessService allAccessService,
            ISettingsService settingsService,
            IAnalyticsService analyticsService)
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
            this.settingsService = settingsService;
            this.analyticsService = analyticsService;
            this.ShowAllCommand = new DelegateCommand(this.ShowAll);
            this.StartRadioCommand = new DelegateCommand(this.StartRadio);

            this.NavigateToTopSongs = new DelegateCommand(
                () =>
                {
                    this.analyticsService.SendEvent("ArtistPage", "Execute", "TopSongs");
                    this.navigationService.NavigateTo<IPlaylistPageView>(
                        new PlaylistNavigationRequest(
                            this.BindingModel.ArtistInfo.Artist,
                            this.BindingModel.ArtistInfo.Artist.Title,
                            "Artist Top Songs",
                            this.BindingModel.ArtistInfo.TopSongs));
                });

            this.NavigateToAlbums = new DelegateCommand(
                () =>
                {
                    this.analyticsService.SendEvent("ArtistPage", "Execute", "ArtistAlbums");
                    this.navigationService.NavigateTo<IPlaylistsPageView>(
                        new PlaylistNavigationRequest(
                            this.BindingModel.ArtistInfo.Artist,
                            this.BindingModel.ArtistInfo.Artist.Title,
                            "Artist Albums",
                            this.BindingModel.ArtistInfo.GoogleAlbums.Cast<IPlaylist>().ToList()));
                });

            this.NavigateToArtists = new DelegateCommand(
               () =>
               {
                   this.analyticsService.SendEvent("ArtistPage", "Execute", "RelatedArtists");
                   this.navigationService.NavigateTo<IPlaylistsPageView>(
                       new PlaylistNavigationRequest(
                           this.BindingModel.ArtistInfo.Artist,
                           this.BindingModel.ArtistInfo.Artist.Title,
                           "Related Artists",
                           this.BindingModel.ArtistInfo.RelatedArtists.Cast<IPlaylist>().ToList()));
               });

            this.ReadMoreCommand = new DelegateCommand(
                () =>
                {
                    if (this.BindingModel.Artist != null && !string.IsNullOrEmpty(this.BindingModel.Artist.Bio))
                    {
                        this.MainFrame.ShowPopup<IReadMorePopup>(PopupRegion.Full, this.BindingModel.Artist.Bio);
                    }
                });
        }

        public DelegateCommand ShowAllCommand { get; set; }

        public DelegateCommand StartRadioCommand { get; set; }

        public DelegateCommand NavigateToTopSongs { get; set; }

        public DelegateCommand NavigateToAlbums { get; set; }

        public DelegateCommand NavigateToArtists { get; set; }

        public DelegateCommand ReadMoreCommand { get; set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.Artist = null;
            this.BindingModel.Albums = null;
            this.BindingModel.Collections = null;
            this.BindingModel.ArtistInfo = null;
            this.BindingModel.IsAllAccessLoading = false;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken)
        {
            var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
            if (request == null || request.PlaylistType != PlaylistType.Artist)
            {
                throw new NotSupportedException("Request should be PlaylistNavigationRequest and playlist type should be artist.");
            }

            this.BindingModel.Artist = null;
            this.BindingModel.Albums = null;
            this.BindingModel.Collections = null;
            this.BindingModel.ArtistInfo = null;
            this.BindingModel.IsAllAccessLoading = false;

            Artist artist = request.Playlist as Artist;

            if (artist != null && artist.ArtistId == 0)
            {
                artist = (await this.artistsRepository.FindByGoogleIdAsync(artist.GoogleArtistId)) ?? artist;
            }
            else
            {
                artist = await this.playlistsService.GetRepository<Artist>().GetAsync(request.PlaylistId);
            }

            this.BindingModel.Artist = artist;

            if (artist != null && artist.ArtistId > 0)
            {
                this.BindingModel.Albums = (await this.albumsRepository.GetArtistAlbumsAsync(artist.Id)).Cast<IPlaylist>().ToList();
                this.BindingModel.Collections = (await this.albumsRepository.GetArtistCollectionsAsync(artist.Id)).Cast<IPlaylist>().ToList();
            }

            if (this.applicationStateService.IsOnline() && this.settingsService.GetIsAllAccessAvailable())
            {
                this.LoadAllAccessSongs(artist, cancellationToken);
            }
        }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            if (this.BindingModel.Artist.ArtistId > 0)
            { 
                yield return new CommandMetadata(CommandIcon.List, this.resources.GetString("Toolbar_ShowAllButton"), this.ShowAllCommand);
            }

            if (this.applicationStateService.IsOnline() && this.BindingModel.Artist != null && !string.IsNullOrEmpty(this.BindingModel.Artist.GoogleArtistId))
            {
                yield return new CommandMetadata(CommandIcon.Radio, this.settingsService.GetIsAllAccessAvailable() ? "Start radio" : "Start instant mix", this.StartRadioCommand);
            }
        }

        private async void LoadAllAccessSongs(Artist artist, CancellationToken cancellationToken)
        {
            try
            {
                await this.Dispatcher.RunAsync(
                    () =>
                    {
                        this.BindingModel.IsAllAccessLoading = true;
                    });

                var info = await this.allAccessService.GetArtistInfoAsync(artist, cancellationToken);
                if (info != null)
                {
                    await this.Dispatcher.RunAsync(
                        () =>
                        {
                            this.BindingModel.ArtistInfo = info;
                        });
                }

                await this.Dispatcher.RunAsync(
                    () =>
                    {
                        this.BindingModel.IsAllAccessLoading = false;
                    });
            }
            catch (OperationCanceledException exception)
            {
                this.Logger.Debug(exception, "Load All Access Songs cancelled");
            }
        }

        private void ShowAll()
        {
            this.analyticsService.SendEvent("ArtistPage", "Execute", "ShowAll");
            this.NavigateToShowAllArtistsSongs();
        }

        private async void StartRadio()
        {
            if (!this.IsDataLoading)
            {
                this.analyticsService.SendEvent("ArtistPage", "Execute", "StartRadio");
                await this.Dispatcher.RunAsync(() => this.IsDataLoading = true);

                var radio = await this.radioStationsService.CreateAsync(this.BindingModel.Artist);

                if (radio != null)
                {
                    if (await this.playQueueService.PlayAsync(radio.Item1, radio.Item2, -1))
                    {
                        await this.Dispatcher.RunAsync(() => this.IsDataLoading = false);

                        this.navigationService.NavigateToPlaylist(radio.Item1);
                    }
                }
            }
        }

        private void NavigateToShowAllArtistsSongs()
        {
            if (this.BindingModel.Artist != null)
            {
                this.navigationService.NavigateTo<IPlaylistPageView>(new PlaylistNavigationRequest(this.BindingModel.Artist));
            }
        }
    }
}