// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Views;

    public class AlbumPageViewPresenter : PagePresenterBase<IAlbumPageView, AlbumPageViewBindingModel>
    {
        public AlbumPageViewPresenter(IDependencyResolverContainer container)
            : base(container)
        {
        }

        protected override void LoadData(NavigatedToEventArgs navigatedToEventArgs)
        {
            var album = navigatedToEventArgs.Parameter as Album;
            if (album == null)
            {
                throw new NotSupportedException("Current view supports only album-playlists.");
            }

            this.BindingModel.Album = album;
        }
    }
}