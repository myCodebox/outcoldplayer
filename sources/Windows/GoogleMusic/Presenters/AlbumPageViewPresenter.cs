// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class AlbumPageViewPresenter : PlaylistPageViewPresenterBase<IAlbumPageView, AlbumBindingModel>
    {
        private readonly IPlaylistCollectionsService playlistCollectionsService;

        public AlbumPageViewPresenter(
            IDependencyResolverContainer container,
            IPlaylistCollectionsService playlistCollectionsService)
            : base(container)
        {
            this.playlistCollectionsService = playlistCollectionsService;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var song = navigatedToEventArgs.Parameter as SongBindingModel;
            if (song != null)
            {
                var albums = await this.playlistCollectionsService.GetCollection<AlbumBindingModel>().GetAllAsync();
                var album = albums.FirstOrDefault(x => x.Songs.Contains(song));

                this.BindingModel.Playlist = album;
                this.BindingModel.SelectedSongIndex = album.Songs.IndexOf(song);
            }
            else
            {
                await base.LoadDataAsync(navigatedToEventArgs);
            }
        }
    }
}