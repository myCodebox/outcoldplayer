// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Controls;

    public interface IPlaylistsView : IView
    {
    }

    public sealed partial class PlaylistsView : ViewBase, IPlaylistsView
    {
        public PlaylistsView()
        {
            this.InitializePresenter<PlaylistsViewPresenter>();
            this.InitializeComponent();
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            this.Presenter<PlaylistsViewPresenter>().ItemClick(e.ClickedItem as PlaylistBindingModel);
        }
    }
}
