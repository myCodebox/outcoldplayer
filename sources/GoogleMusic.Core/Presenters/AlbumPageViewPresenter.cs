// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class AlbumPageViewPresenter : PlaylistPageViewPresenterBase<IAlbumPageView>
    {
        private readonly IAllAccessService allAccessService;

        private readonly IApplicationStateService applicationStateService;

        private readonly INavigationService navigationService;

        private readonly IPlayQueueService playQueueService;

        private readonly IRadioStationsService radioStationsService;

        private readonly ISettingsService settingsService;

        private readonly IAnalyticsService analyticsService;

        private Tuple<Album, IList<Song>> allAccessAlbum;

        public AlbumPageViewPresenter(
            IDependencyResolverContainer container,
            IAllAccessService allAccessService,
            IApplicationStateService applicationStateService,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            IRadioStationsService radioStationsService,
            ISettingsService settingsService,
            IAnalyticsService analyticsService)
            : base(container)
        {
            this.allAccessService = allAccessService;
            this.applicationStateService = applicationStateService;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;
            this.radioStationsService = radioStationsService;
            this.settingsService = settingsService;
            this.analyticsService = analyticsService;
            this.NavigateToArtistCommand = new DelegateCommand(this.NavigateToArtist);

            this.StartRadioCommand = new DelegateCommand(this.StartRadio);

            this.ReadMoreCommand = new DelegateCommand(
                () =>
                {
                    if (this.BindingModel.Playlist is Album 
                        && !string.IsNullOrEmpty(((Album)this.BindingModel.Playlist).Description))
                    {
                        this.MainFrame.ShowPopup<IReadMorePopup>(PopupRegion.Full, ((Album)this.BindingModel.Playlist).Description);
                    }
                });

            this.NavigateToAllAccessAlbumCommand = new DelegateCommand(
                () =>
                {
                    this.analyticsService.SendEvent("AlbumPage", "Execute", "NavigateToAllAccessAlbum");

                    if (this.allAccessAlbum != null && this.allAccessAlbum.Item2 != null)
                    {
                        this.navigationService.NavigateToPlaylist(
                            new PlaylistNavigationRequest(this.allAccessAlbum.Item1, this.allAccessAlbum.Item2)
                            {
                                ForceToShowAllAccess = true
                            });
                    }
                });
        }

        public DelegateCommand StartRadioCommand { get; set; }

        public DelegateCommand NavigateToArtistCommand { get; set; }

        public DelegateCommand NavigateToAllAccessAlbumCommand { get; set; }

        public DelegateCommand ReadMoreCommand { get; set; }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            if (this.applicationStateService.IsOnline() 
                && this.BindingModel.Playlist != null 
                && !string.IsNullOrEmpty(((Album)this.BindingModel.Playlist).GoogleAlbumId))
            {
                yield return new CommandMetadata(CommandIcon.Radio, "Start radio", this.StartRadioCommand);

                if (this.allAccessAlbum != null && this.settingsService.GetIsAllAccessAvailable())
                {
                    yield return new CommandMetadata(CommandIcon.Web, "All Access Album", this.NavigateToAllAccessAlbumCommand);
                }
            }
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.allAccessAlbum = null;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken)
        {
            var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
            if (request != null 
                && request.Playlist != null 
                && (((Album)request.Playlist).AlbumId == 0
                || request.ForceToShowAllAccess))
            {
                Tuple<Album, IList<Song>> result = null;

                if (request.Songs != null)
                {
                    result = new Tuple<Album, IList<Song>>((Album)request.Playlist, request.Songs);
                }
                else
                {
                    result = await this.allAccessService.GetAlbumAsync((Album)request.Playlist, cancellationToken);
                }

                if (result != null)
                {
                    await this.Dispatcher.RunAsync(
                        () =>
                        {
                            this.BindingModel.Songs = result.Item2;
                            this.BindingModel.Playlist = result.Item1;
                            this.BindingModel.Title = result.Item1.Title;
                            this.BindingModel.Subtitle = PlaylistTypeEx.GetTitle(result.Item1.PlaylistType);
                        });
                }
            }
            else
            {
                await base.LoadDataAsync(navigatedToEventArgs, cancellationToken);

                Album album = this.BindingModel.Playlist as Album;
                if (album != null && string.IsNullOrEmpty(album.Description) && !string.IsNullOrEmpty(album.GoogleAlbumId))
                {
                    var result = await this.allAccessService.GetAlbumAsync(album, cancellationToken);
                    if (result != null)
                    {
                        this.allAccessAlbum = result;
                        await this.Dispatcher.RunAsync(
                            () =>
                            {
                                this.BindingModel.Playlist = result.Item1;
                            });
                    }
                }
            }
        }

        private void NavigateToArtist()
        {
            var album = this.BindingModel.Playlist as Album;
            if (album != null && album.Artist != null)
            {
                if (this.applicationStateService.IsOnline() || album.Artist.ArtistId > 0)
                {
                    this.navigationService.NavigateToPlaylist(album.Artist);
                }
            }
        }

        private async void StartRadio()
        {
            if (!this.IsDataLoading)
            {
                this.analyticsService.SendEvent("AlbumPage", "Execute", "StartRadio");

                await this.Dispatcher.RunAsync(() => this.IsDataLoading = true);

                var radio = await this.radioStationsService.CreateAsync((Album)this.BindingModel.Playlist);

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
    }
}