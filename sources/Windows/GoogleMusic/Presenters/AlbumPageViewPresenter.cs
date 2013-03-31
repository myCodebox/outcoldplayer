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
        private readonly ISongsRepository songsRepository;

        public AlbumPageViewPresenter(
            IDependencyResolverContainer container,
            ISongsRepository songsRepository)
            : base(container)
        {
            this.songsRepository = songsRepository;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            if (navigatedToEventArgs.Parameter is int)
            {
                int songId = (int)navigatedToEventArgs.Parameter;
                Song song = await this.songsRepository.GetSongAsync(songId);

                await base.LoadDataAsync(
                        new NavigatedToEventArgs(
                            navigatedToEventArgs.View,
                            navigatedToEventArgs.State,
                            new PlaylistNavigationRequest(PlaylistType.Album, song.Album.Id),
                            navigatedToEventArgs.IsNavigationBack));
                
                var songBindingModel = this.BindingModel.SongsBindingModel.Songs.FirstOrDefault(s => s.Metadata.SongId == songId);
                if (songBindingModel != null)
                {
                    this.BindingModel.SongsBindingModel.SelectedItems.Add(songBindingModel);
                }
            }
            else
            {
                await base.LoadDataAsync(navigatedToEventArgs);
            }
        }
    }
}