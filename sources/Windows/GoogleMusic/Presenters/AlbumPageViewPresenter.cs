// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
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

        public AlbumPageViewPresenter(
            IDependencyResolverContainer container,
            IApplicationResources resources,
            IAllAccessService allAccessService,
            IAlbumsRepository albumsRepository,
            IApplicationStateService applicationStateService,
            INavigationService navigationService)
            : base(container)
        {
            this.resources = resources;
            this.allAccessService = allAccessService;
            this.albumsRepository = albumsRepository;
            this.applicationStateService = applicationStateService;
            this.navigationService = navigationService;
            this.NavigateToArtistCommand = new DelegateCommand(this.NavigateToArtist);


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

        public DelegateCommand NavigateToArtistCommand { get; set; }

        public DelegateCommand ReadMoreCommand { get; set; }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            Album album = null;
            var songId = navigatedToEventArgs.Parameter as string;
            if (songId != null)
            {
                album = await this.albumsRepository.FindSongAlbumAsync(songId);

                await base.LoadDataAsync(
                        new NavigatedToEventArgs(
                            navigatedToEventArgs.View,
                            navigatedToEventArgs.State,
                            new PlaylistNavigationRequest(album, songId),
                            navigatedToEventArgs.IsNavigationBack));
            }
            else
            {
                var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
                if (request != null && request.Playlist != null && ((Album)request.Playlist).AlbumId == 0)
                {
                    var result = await this.allAccessService.GetAlbumAsync((Album)request.Playlist);

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
                    await base.LoadDataAsync(navigatedToEventArgs);
                }
            }

            album = this.BindingModel.Playlist as Album;
            if (album != null && string.IsNullOrEmpty(album.Description) && !string.IsNullOrEmpty(album.GoogleAlbumId))
            {
                var result = await this.allAccessService.GetAlbumAsync(album);
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
    }
}