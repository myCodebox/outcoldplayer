// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;

    using Windows.UI.Xaml.Controls;

    public interface IAddToPlaylistPopupView : IPopupView
    {
    }

    public sealed partial class AddToPlaylistPopupView : PopupViewBase, IAddToPlaylistPopupView
    {
        private AddToPlaylistPopupViewPresenter presenter;

        public AddToPlaylistPopupView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<AddToPlaylistPopupViewPresenter>();
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            var playlist = e.ClickedItem as AddToPlaylistPopupViewPresenter.AddToSongMusicPlaylist;
            if (playlist != null)
            {
                this.presenter.AddToPlaylist(playlist);
            }
        }
    }
}
