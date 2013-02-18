// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.BindingModels;

    public abstract class PlaylistsViewPresenterBase<TView> : PagePresenterBase<TView> where TView : IPageView
    {
        private readonly INavigationService navigationService;

        protected PlaylistsViewPresenterBase(
            IDependencyResolverContainer container)
            : base(container)
        {
            this.navigationService = container.Resolve<INavigationService>();
        }

        public virtual void ItemClick(PlaylistBindingModel playlistBindingModel)
        {
            this.Logger.Debug("ItemClick.");
            if (playlistBindingModel != null)
            {
                this.Logger.Debug("ItemClick. Playlist '{0}'.", playlistBindingModel.Playlist.Title);
                this.navigationService.NavigateToView<PlaylistViewResolver>(playlistBindingModel.Playlist);
            }
        }
    }
}