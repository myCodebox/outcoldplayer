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

    public class AlbumPageViewPresenter : PlaylistPageViewPresenterBase<IAlbumPageView, Album>
    {
        private readonly IApplicationResources resources;
        private readonly IAllAccessService allAccessService;
        private readonly IAlbumsRepository albumsRepository;

        public AlbumPageViewPresenter(
            IDependencyResolverContainer container,
            IApplicationResources resources,
            IAllAccessService allAccessService,
            IAlbumsRepository albumsRepository)
            : base(container)
        {
            this.resources = resources;
            this.allAccessService = allAccessService;
            this.albumsRepository = albumsRepository;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var songId = navigatedToEventArgs.Parameter as string;
            if (songId != null)
            {
                Album album = await this.albumsRepository.FindSongAlbumAsync(songId);

                await base.LoadDataAsync(
                        new NavigatedToEventArgs(
                            navigatedToEventArgs.View,
                            navigatedToEventArgs.State,
                            new PlaylistNavigationRequest(PlaylistType.Album, album.Id, songId),
                            navigatedToEventArgs.IsNavigationBack));
            }
            else
            {
                var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
                if (request != null && request.Playlist != null && ((Album)request.Playlist).AlbumId == 0)
                {
                    var result = await this.allAccessService.GetAlbumAsync((Album)request.Playlist);

                    this.BindingModel.Songs = result.Item2;
                    this.BindingModel.Playlist = result.Item1;
                    this.BindingModel.Type = this.resources.GetTitle(result.Item1.PlaylistType);

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
        }
    }
}