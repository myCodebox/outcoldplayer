// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;
    using OutcoldSolutions.GoogleMusic.Views;

    public class AlbumPageViewPresenter : PlaylistPageViewPresenterBase<IAlbumPageView, Album>
    {
        public AlbumPageViewPresenter(
            IDependencyResolverContainer container)
            : base(container)
        {
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var song = navigatedToEventArgs.Parameter as SongBindingModel;
            if (song != null)
            {
                await base.LoadDataAsync(
                        new NavigatedToEventArgs(
                            navigatedToEventArgs.View,
                            navigatedToEventArgs.State,
                            new PlaylistNavigationRequest()
                                {
                                    PlaylistId = song.Metadata.Album.Id,
                                    PlaylistType = PlaylistType.Album
                                },
                            navigatedToEventArgs.IsNavigationBack));

                this.BindingModel.SelectedSongIndex = this.BindingModel.Songs.IndexOf(new SongBindingModel(song.Metadata));
            }
            else
            {
                await base.LoadDataAsync(navigatedToEventArgs);
            }
        }
    }
}