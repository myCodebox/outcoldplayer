// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Views;

    public class AlbumPageViewPresenter : PlaylistPageViewPresenterBase<IAlbumPageView, Album>
    {
        private readonly IAlbumsRepository albumsRepository;

        public AlbumPageViewPresenter(
            IDependencyResolverContainer container,
            IAlbumsRepository albumsRepository)
            : base(container)
        {
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
                            new PlaylistNavigationRequest(PlaylistType.Album, album.Id),
                            navigatedToEventArgs.IsNavigationBack));
                
                var songBindingModel = this.BindingModel.SongsBindingModel.Songs.FirstOrDefault(s => s.Metadata.SongId == songId);
                if (songBindingModel != null)
                {
                    await this.Dispatcher.RunAsync(() => this.BindingModel.SongsBindingModel.SelectedItems.Add(songBindingModel));
                }
            }
            else
            {
                await base.LoadDataAsync(navigatedToEventArgs);
            }
        }
    }
}