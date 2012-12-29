// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Views;

    public abstract class PlaylistsViewPresenterBase<TView> : ViewPresenterBase<TView> where TView : IView
    {
        private readonly INavigationService navigationService;

        protected PlaylistsViewPresenterBase(
            IDependencyResolverContainer container, 
            TView view)
            : base(container, view)
        {
            this.navigationService = container.Resolve<INavigationService>();
        }

        public void ItemClick(PlaylistBindingModel playlistBindingModel)
        {
            this.Logger.Debug("ItemClick.");
            if (playlistBindingModel != null)
            {
                this.Logger.Debug("ItemClick. Playlist '{0}'.", playlistBindingModel.Playlist.Title);
                this.navigationService.NavigateTo<IPlaylistView>(playlistBindingModel.Playlist);
            }
        }
    }
}