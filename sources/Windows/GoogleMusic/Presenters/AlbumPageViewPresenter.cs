// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class AlbumPageViewPresenter : PlaylistPageViewPresenterBase<IAlbumPageView>
    {
        private readonly IApplicationResources resources;
        private readonly IAllAccessService allAccessService;
        private readonly IAlbumsRepository albumsRepository;

        private readonly IApplicationStateService applicationStateService;

        private readonly INavigationService navigationService;

        private readonly IPlayQueueService playQueueService;

        private readonly IRadioStationsService radioStationsService;

        public AlbumPageViewPresenter(
            IDependencyResolverContainer container,
            IApplicationResources resources,
            IAllAccessService allAccessService,
            IAlbumsRepository albumsRepository,
            IApplicationStateService applicationStateService,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            IRadioStationsService radioStationsService)
            : base(container)
        {
            this.resources = resources;
            this.allAccessService = allAccessService;
            this.albumsRepository = albumsRepository;
            this.applicationStateService = applicationStateService;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;
            this.radioStationsService = radioStationsService;
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
        }

        public DelegateCommand StartRadioCommand { get; set; }

        public DelegateCommand NavigateToArtistCommand { get; set; }

        public DelegateCommand ReadMoreCommand { get; set; }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            if (this.applicationStateService.IsOnline() 
                && this.BindingModel.Playlist != null 
                && !string.IsNullOrEmpty(((Album)this.BindingModel.Playlist).GoogleAlbumId))
            {
                yield return new CommandMetadata(CommandIcon.Radio, "Start radio", this.StartRadioCommand);
            }
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken)
        {
            Album album = null;
            
            var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
            if (request != null && request.Playlist != null && ((Album)request.Playlist).AlbumId == 0)
            {
                var result = await this.allAccessService.GetAlbumAsync((Album)request.Playlist, cancellationToken);

                if (result != null)
                {
                    await this.Dispatcher.RunAsync(
                        () =>
                        {
                            this.BindingModel.Songs = result.Item2;
                            this.BindingModel.Playlist = result.Item1;
                            this.BindingModel.Title = result.Item1.Title;
                            this.BindingModel.Subtitle = this.resources.GetTitle(result.Item1.PlaylistType);
                        });
                }


                if (!string.IsNullOrEmpty(request.SongId))
                {
                    this.EventAggregator.Publish(new SelectSongByIdEvent(request.SongId));
                }
            }
            else
            {
                await base.LoadDataAsync(navigatedToEventArgs, cancellationToken);
            }

            album = this.BindingModel.Playlist as Album;
            if (album != null && string.IsNullOrEmpty(album.Description) && !string.IsNullOrEmpty(album.GoogleAlbumId))
            {
                var result = await this.allAccessService.GetAlbumAsync(album, cancellationToken);
                if (result != null)
                {
                    await this.Dispatcher.RunAsync(
                        () =>
                        {
                            this.BindingModel.Playlist = result.Item1;
                        });
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
                await this.Dispatcher.RunAsync(() => this.IsDataLoading = true);

                var radio = await this.radioStationsService.CreateAsync((Album)this.BindingModel.Playlist);

                if (radio != null)
                {
                    await this.playQueueService.PlayAsync(radio.Item1, radio.Item2, -1);

                    await this.Dispatcher.RunAsync(() => this.IsDataLoading = false);

                    this.navigationService.NavigateToPlaylist(radio.Item1);
                }
            }
        }
    }
}